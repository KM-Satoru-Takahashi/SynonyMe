using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.ObjectModel;
using System.IO;
using SynonyMe.View;
using SynonyMe.ViewModel;
using GongSolutions.Wpf.DragDrop;
using ICSharpCode.AvalonEdit;

namespace SynonyMe.Model
{
    internal class MainWindowModel
    {
        #region field

        /// <summary>ViewModel</summary>
        private ViewModel.MainWindowVM _viewModel = null;

        /// <summary>本プロセスで処理対象となるファイル拡張子一覧</summary>
        /// <remarks>ここに含まれない拡張子のファイルは読み込み時に弾かれる</remarks>
        private static readonly string[] PROCESS_TARGET_FILE_EXTENSIONS = new string[]
        {
            ".txt"
        };

        /// <summary>検索結果を何個まで表示するか(何個まで検索対象とするか)</summary>
        /// <remarks>将来的に設定ファイルで外出しする予定</remarks>
        private const int SEARCH_RESULT_DISPLAY_NUMBER = 100;

        #endregion

        #region method

        /// <summary>コンストラクタ</summary>
        /// <param name="viewModel">メンバに保持するVM</param>
        internal MainWindowModel(ViewModel.MainWindowVM viewModel)
        {
            _viewModel = viewModel;
        }

        /// <summary>ドラッグオーバー中のファイルがドロップ可能かを調べる</summary>
        /// <returns>true:ドロップ可能、false:ドロップ不可能(何か1つでも不可能な場合)</returns>
        internal bool CanDrop(IDropInfo dropInfo)
        {
            List<string> dragOverFilePathList = new List<string>();

            if (ConvertDropInfoToPathList(dropInfo, out dragOverFilePathList) == false)
            {
                return false;
            }

            if (dragOverFilePathList == null || dragOverFilePathList.Any() == false)
            {
                return false;
            }

            foreach (string filePath in dragOverFilePathList)
            {
                // 1つでも対象外のファイルがあれば弾く
                if (IsTargetFile(filePath) == false)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>画面表示テキストを取得する</summary>
        /// <param name="dropInfo">ドロップされたファイル</param>
        /// <returns>画面表示テキスト情報</returns>
        internal List<string> GetDisplayText(IDropInfo dropInfo)
        {
            if (dropInfo == null)
            {
                return null;
            }

            List<string> filePathList = new List<string>();
            if (ConvertDropInfoToPathList(dropInfo, out filePathList) == false)
            {
                return null;
            }

            // 現状、表示できるファイルは1つだけなので先頭のものを使用
            // filePathListのnull/AnyチェックはConbertDropInfoToPathList内で行っている
            return GetTextFromFilePath(filePathList);
        }

        /// <summary>表示対象ファイルのパス(絶対パス)を取得する</summary>
        /// <param name="dropInfo">ドロップされたファイル情報</param>
        /// <returns>対象ファイルの絶対パス</returns>
        internal List<string> GetDisplayTextFilePath(IDropInfo dropInfo)
        {
            if (dropInfo == null)
            {
                return null;
            }

            List<string> filePathList = new List<string>();
            if (ConvertDropInfoToPathList(dropInfo, out filePathList) == false)
            {
                return null;
            }

            // 現状、表示できるファイルは1つだけなので先頭のものを使用
            // filePathListのnull/AnyチェックはConbertDropInfoToPathList内で行っている
            return filePathList;
        }

        /// <summary>渡されたファイル情報に基づいて保存処理を実行する</summary>
        /// <param name="filePath">保存対象ファイルパス</param>
        /// <param name="displayText">保存したいテキスト情報</param>
        /// <returns>true:成功, false:失敗</returns>
        internal bool Save(string filePath, string displayText)
        {
            if (string.IsNullOrEmpty(filePath) ||
               string.IsNullOrEmpty(displayText))
            {
                return false;
            }

            TextEditor editor = _viewModel.TextEditor;
            try
            {
                editor.Load(filePath);
                editor.Text = displayText;
                editor.Save(filePath);
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        /// <summary>類語ウィンドウを開く</summary>
        internal void OpenSynonymWindow()
        {
            Manager.WindowManager.OpenSubWindow(CommonLibrary.Define.SubWindowName.SynonymWindow);
        }

        /// <summary>TextEditorを使用して与えられたファイルパスから読み込んだ文字列を返す</summary>
        /// <param name="filePath">読み込み対象のファイルパス</param>
        /// <returns>読み込んだファイルの全テキスト</returns>
        private List<string> GetTextFromFilePath(List<string> filePathList)
        {
            if (filePathList == null || filePathList.Any() == false)
            {
                return null;
            }

            List<string> textList = new List<string>();
            foreach (string filePath in filePathList)
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    continue;
                }

                string text = null;
                if (Load(filePath, out text))
                {
                    textList.Add(text);
                }
            }

            return textList;
        }

        /// <summary>渡されたファイルパスからテキストファイルを読み込む</summary>
        /// <param name="filePath">読み込み対象のファイルパス</param>
        /// <param name="text">読み込んだファイルの全テキスト</param>
        /// <returns>true:成功, false:失敗</returns>
        private bool Load(string filePath, out string text)
        {
            text = null;
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            TextEditor textEditor = _viewModel.TextEditor;
            try
            {
                textEditor.Load(filePath);
            }
            catch (Exception e)
            {
                return false;
            }

            text = textEditor.Text;
            return true;
        }

        /// <summary>DropInfoをファイルパスのリストに変換する</summary>
        /// <param name="dropInfo">変換元ファイル</param>
        /// <param name="filePathList">変換後のファイルパス(絶対パス)リスト</param>
        /// <returns>true:成功, false:失敗/異常</returns>
        private bool ConvertDropInfoToPathList(IDropInfo dropInfo, out List<string> filePathList)
        {
            filePathList = new List<string>();

            if (dropInfo == null)
            {
                return false;
            }

            DataObject dragOverFiles = (DataObject)dropInfo.Data;
            if (dragOverFiles == null)
            {
                return false;
            }

            System.Collections.Specialized.StringCollection dragOverFileList = dragOverFiles.GetFileDropList();
            if (dragOverFileList == null || dragOverFileList.Count < 1)
            {
                return false;
            }

            filePathList = dragOverFileList.Cast<string>().ToList();
            if (filePathList == null || filePathList.Any() == false)
            {
                return false;
            }

            return true;
        }

        /// <summary>渡されたファイルパスが処理対象のファイルかどうかを判定する</summary>
        /// <param name="filePath">チェック対象ファイルパス</param>
        /// <returns>true:処理対象のファイル, false:それ以外のファイル</returns>
        /// <remarks>ファイルパスは拡張子をチェックするので、絶対・相対いずれも可</remarks>
        private bool IsTargetFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            string extension = Path.GetExtension(filePath);

            if (PROCESS_TARGET_FILE_EXTENSIONS.Contains(extension))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>検索処理を実施する</summary>
        /// <param name="searchWord">検索語句</param>
        /// <param name="targetText">検索対象の文章</param>
        /// <param name="margin">検索結果として、対象語句の前後何文字を含めるか</param>
        /// <returns>文章内の検索対象index, margin含めた検索結果のdictionary</returns>
        internal Dictionary<int, string> SearchAllWordsInText(string searchWord, string targetText, int margin)
        {
            // check args
            if (string.IsNullOrEmpty(searchWord))
            {
                return null;
            }
            else if (string.IsNullOrEmpty(targetText))
            {
                return null;
            }
            else if (margin < 0 /*最大値は現状未定、最小値も設定ファイルや定数で外出しする予定だが、現状ハードコーティングとする*/)
            {
                return null;
            }

            // 文書中で該当するインデックスを一旦入れておくリストを用意
            List<int> searchResultIndex = new List<int>();

            // 1箇所目をまず探す
            int foundIndex = targetText.IndexOf(searchWord);
            if (foundIndex < 0)
            {
                // 検索したが何もない場合はエラーではないので空のdicを戻すようにする
                return new Dictionary<int, string>();
            }

            // 他の箇所を繰り返し探していく
            int resultCount = 1;
            while (0 <= foundIndex) // 該当がなくなると検索結果インデックスは-1が戻ってくる
            {
                // 検索結果は規定値まで
                if (SEARCH_RESULT_DISPLAY_NUMBER < resultCount)
                {
                    break;
                }

                // 最初に[前回の検索結果インデックス]をリストに追加しておく
                // 1箇所目も登録される
                searchResultIndex.Add(foundIndex);

                // 次の検索位置は「前の検索位置」に「検索対象の語句の長さ」を足した地点
                int nextIndex = foundIndex + searchWord.Length;
                if (nextIndex < targetText.Length)
                {
                    foundIndex = targetText.IndexOf(searchWord, nextIndex);
                }
                else
                {
                    // 文章の長さを超えるなら、検索しない
                    break;
                }

                ++resultCount;
            }

            // 実際にテキストから、Viewに表示対象となる語句領域を切り取っていく
            // インデックス分だけ必ずあるはず
            string[] searchResultWordList = new string[searchResultIndex.Count];
            for (int targetIndex = 0; targetIndex < searchResultIndex.Count; ++targetIndex)
            {
                // 手前側マージン
                int frontMargin = searchResultIndex[targetIndex] - margin;
                // 後ろ側マージン→インデックス＋検索対象語句＋マージン
                int behindMargin = searchResultIndex[targetIndex] + searchWord.Length + margin;

                // 後ろのマージンがなくても、最後の検索とは限らないので、foreachは続けること
                // 例：「あああああああ」で「あ」だけを検索した場合
                if (frontMargin < 0 && targetText.Length < behindMargin + 1) // LengthとIndexを比較するのでIndexに+1しておく
                {
                    // 手前に規定値分のマージンがなく、後ろにも規定値分のマージンがない場合
                    // 「文字列の最初～文字列の最後」までを切り取る→検索対象の文字列をそのまま入れ込む
                    searchResultWordList[targetIndex] = targetText;
                }
                else if (frontMargin < 0)
                {
                    // 手前に規定値分のマージンがなく、後ろには規定値分のマージンがある場合
                    // 「文字列の最初～インデックス＋検索対象語句＋後ろのマージン」だけ切り取る
                    searchResultWordList[targetIndex] = targetText.Substring(0, searchWord.Length + margin); // substringの第2引数は切り取る文字数
                }
                else if (targetText.Length < behindMargin + 1)
                {
                    // 手前に規定値分のマージンがあり、後ろには規定値分のマージンがない場合
                    // 「手前のマージン～文字列の最後」までを切り取る
                    searchResultWordList[targetIndex] = targetText.Substring(frontMargin);

                }
                else
                {
                    // 手前に規定値分のマージンがあり、後ろにも規定値分のマージンがある場合
                    // 「手前のマージン～インデックス＋検索対象語句＋後ろのマージン」だけ切り取る
                    searchResultWordList[targetIndex] = targetText.Substring(frontMargin, searchWord.Length + margin);
                }
            }

            // 最終的にDictionaryで返せばよくない？
            Dictionary<int/*index*/, string/*result*/> searchResultIndexWordPairs = new Dictionary<int, string>();
            for (int i = 0; i < searchResultIndex.Count; ++i)
            {
                searchResultIndexWordPairs.Add(searchResultIndex[i], searchResultWordList[i]);
            }

            return searchResultIndexWordPairs;
        }


        #endregion
    }
}
