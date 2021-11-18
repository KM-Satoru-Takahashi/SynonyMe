using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.IO;
using SynonyMe.View;
using SynonyMe.ViewModel;
using GongSolutions.Wpf.DragDrop;
using ICSharpCode.AvalonEdit;
using SynonyMe.CommonLibrary.Log;
using SynonyMe.Model.Manager;
using System.Windows.Forms;

namespace SynonyMe.Model
{
    internal class MainWindowModel
    {
        #region field

        private const string CLASS_NAME = "MainWindowModel";

        /// <summary>ViewModel</summary>
        private MainWindowVM _viewModel = null;

        private FileAccessManager _fileAccessManager = null;


        private DialogManager _dialogManager = null;


        /// <summary>本プロセスで処理対象となるファイル拡張子一覧</summary>
        /// <remarks>ここに含まれない拡張子のファイルは読み込み時に弾かれる</remarks>
        private static readonly string[] PROCESS_TARGET_FILE_EXTENSIONS = new string[]
        {
            ".txt"
        };

        /// <summary>検索結果を何個まで表示するか(何個まで検索対象とするか)</summary>
        /// <remarks>将来的に設定ファイルで外出しする予定</remarks>
        private const int SEARCH_RESULT_DISPLAY_NUMBER = 100;


        private AvalonEdit.Highlight.HighlightManager _highlightManager = null;

        #region property

        /// <summary>true:表示中のテキストが編集済み, false:未編集または保存済み</summary>
        internal bool IsModifiedOrNewFile
        {
            private get
            {
                if (TextEditor == null)
                {
                    return false;
                }

                return TextEditor.IsModified;
            }
            set
            {
                if (_viewModel != null && TextEditor != null)
                {
                    TextEditor.IsModified = value;
                }
            }
        }

        /// <summary>文章1つにつき1つ割り当てられるAvalonEditインスタンス</summary>
        /// <remarks>複数文章を表示する改修を行う場合、Dictionaryで文章とTextEditorを紐付けて管理する必要あり</remarks>
        internal TextEditor TextEditor { get; } = new TextEditor();

        /// <summary>画面表示中テキストの絶対パス</summary>
        internal string DisplayTextFilePath;

        #endregion

        #endregion event

        internal event EventHandler UpdateSynonymEvent
        {
            add
            {
                Manager.SynonymManager.UpdateSynonymEvent += value;
            }
            remove
            {
                Manager.SynonymManager.UpdateSynonymEvent -= value;
            }
        }

        #region method

        /// <summary>コンストラクタ</summary>
        /// <param name="viewModel">メンバに保持するVM</param>
        internal MainWindowModel(ViewModel.MainWindowVM viewModel)
        {
            if (viewModel == null)
            {
                Logger.Fatal(CLASS_NAME, "MainWindowModel", "viewModel is null");
                return;
            }

            _viewModel = viewModel;
            _highlightManager = new AvalonEdit.Highlight.HighlightManager(_viewModel.AvalonEditBackGround);
            _fileAccessManager = new FileAccessManager();
            _dialogManager = new DialogManager();
        }


        internal bool ApplyHighlightToTargets(string[] targets)
        {
            if (targets == null || targets.Any() == false)
            {
                Logger.Fatal(CLASS_NAME, "ApplyHighlightToTargets", "targets is null or empty!");
                return false;
            }

            if (_highlightManager == null)
            {
                Logger.Fatal(CLASS_NAME, "ApplyHighlightToTargets", "_highlightManager is null!");
                return false;
            }

            return _highlightManager.UpdateXshdFile(targets);
        }

        /// <summary>指定された語句にハイライトを適用します</summary>
        /// <param name="target">対象語句</param>
        /// <returns>true:成功, false:失敗</returns>
        internal bool ApplyHighlightToTarget(string target)
        {
            if (string.IsNullOrEmpty(target))
            {
                Logger.Fatal(CLASS_NAME, "ApplyHighlightToTarget", "target is null or empty!");
                return false;
            }

            string[] targets = new string[1]
            {
                target
            };

            return ApplyHighlightToTargets(targets);
        }

        /// <summary>ドラッグオーバー中のファイルがドロップ可能かを調べる</summary>
        /// <returns>true:ドロップ可能、false:ドロップ不可能(何か1つでも不可能な場合)</returns>
        internal bool CanDrop(IDropInfo dropInfo)
        {
            List<string> dragOverFilePathList = new List<string>();

            if (ConvertDropInfoToPathList(dropInfo, out dragOverFilePathList) == false)
            {
                Logger.Error(CLASS_NAME, "CanDrop", "ConvertDropInfoToPathList return false!");
                return false;
            }

            if (dragOverFilePathList == null || dragOverFilePathList.Any() == false)
            {
                Logger.Fatal(CLASS_NAME, "CanDrop", "dragOverFilePathList is invalid!");
                return false;
            }

            foreach (string filePath in dragOverFilePathList)
            {
                // 1つでも対象外のファイルがあれば弾く
                if (IsTargetFile(filePath) == false)
                {
                    Logger.Error(CLASS_NAME, "CanDrop", $"there is not target file. file name is {filePath}");
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
                Logger.Fatal(CLASS_NAME, "GetDisplayText", "dropInfo is null!");
                return null;
            }

            List<string> filePathList = new List<string>();
            if (ConvertDropInfoToPathList(dropInfo, out filePathList) == false)
            {
                Logger.Fatal(CLASS_NAME, "GetDisplayText", "ConvertDropInfoToPathList return false!");
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
                Logger.Fatal(CLASS_NAME, "GetDisplayTextFilePath", "dropInfo is null!");
                return null;
            }

            List<string> filePathList = new List<string>();
            if (ConvertDropInfoToPathList(dropInfo, out filePathList) == false)
            {
                Logger.Fatal(CLASS_NAME, "GetDisplayTextInfo", "ConvertDropInfoToPathList return false!");
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
        internal bool Save(string displayText)
        {
            Logger.Info(CLASS_NAME, "Save", $"start. filePath:[{(string.IsNullOrEmpty(DisplayTextFilePath) ? "CreateNewFile!" : DisplayTextFilePath)}]");

            // 名前をつけて保存を実行する
            if (IsModifiedOrNewFile)
            {
                return SaveAs(displayText);
            }

            if (string.IsNullOrEmpty(DisplayTextFilePath) ||
               displayText == null) // displayTextは空文字の場合emptyはあり得る
            {
                Logger.Fatal(CLASS_NAME, "Save", "filePath or displayText is null or empty!");
                return false;
            }

            if(TextEditor==null)
            {
                Logger.Fatal(CLASS_NAME, "Save", "TextEditor is null!");
                return false;
            }

            try
            {
                TextEditor.Text = displayText;
                TextEditor.Save(DisplayTextFilePath);
            }
            catch (Exception e)
            {
                Logger.Fatal(CLASS_NAME, "Save", e.Message);
                return false;
            }

            // AvalonEditの編集済みフラグをOffにする
            IsModifiedOrNewFile = false;

            return true;
        }

        /// <summary>名前をつけて保存</summary>
        /// <returns>true:成功, false:失敗</returns>
        internal bool SaveAs()
        {
            if (TextEditor == null)
            {
                Logger.Fatal(CLASS_NAME, "SaveAs", "TextEditor is null!");
                return false;
            }

            return SaveAs(TextEditor.Text);
        }

        /// <summary>名前をつけて保存</summary>
        /// <param name="displayTest">保存対象テキスト</param>
        /// <returns></returns>
        private bool SaveAs(string displayTest)
        {
            if(TextEditor==null)
            {
                Logger.Fatal(CLASS_NAME, "SaveAs", "TextEditor is null!");
                return false;
            }

            Logger.Info(CLASS_NAME, "SaveAs", "start");

            // ダイアログを開き、保存要求を出す
            if(_dialogManager==null)
            {
                Logger.Fatal(CLASS_NAME, "SaveAs", "_dialogManager is null!");
                return false;
            }

            string saveFilePath = string.Empty;
            bool result = _dialogManager.OpenSaveAsDialog(out saveFilePath);

            // 失敗時はログとエラーダイアログを出す
            if (result == false)
            {
                Logger.Error(CLASS_NAME, "SaveAs", "SaveAs Failed!");

                // todo error dialog
                return false;
            }

            // 成功時はファイルをnullで保存しておく
            if(_fileAccessManager == null)
            {
                Logger.Fatal(CLASS_NAME, "SaveAs", "_fileAccessManager is null!");
                return false;
            }

            if(_fileAccessManager.SaveFile(TextEditor.Text, saveFilePath) == false)
            {
                Logger.Error(CLASS_NAME, "SaveAs", $"SaveFile Failed. saveFilePath:[{saveFilePath}]");
                return false;
            }

            // 保持している、現在開いているファイル情報を更新する
            DisplayTextFilePath = saveFilePath;

            // AvalonEditの編集済みフラグをOffにする
            IsModifiedOrNewFile = false;

            // 名前をつけて保存フラグをOffにする
            IsModifiedOrNewFile = false;
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
                Logger.Fatal(CLASS_NAME, "GetTextFromFilePath", "filePathList is null or empty!");
                return null;
            }

            List<string> textList = new List<string>();
            foreach (string filePath in filePathList)
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    Logger.Error(CLASS_NAME, "GetTextFromFilePath", "filePath is null or empty!");
                    continue;
                }

                string text = null;
                if (Load(filePath, out text))
                {
                    textList.Add(text);
                }
                else
                {
                    Logger.Error(CLASS_NAME, "GetTextFromFilePath", $"Load failed! filePath is {filePath}");
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
                Logger.Error(CLASS_NAME, "Load", "filePath is null or empty!");
                return false;
            }

            try
            {
                TextEditor.Load(filePath);
                TextEditor.Background = System.Windows.Media.Brushes.Red;
            }
            catch (Exception e)
            {
                Logger.Fatal(CLASS_NAME, "Load", e.Message);
                return false;
            }

            text = TextEditor.Text;
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
                Logger.Fatal(CLASS_NAME, "ConvertDropInfoToPathList", "dropInfo is null!");
                return false;
            }

            System.Windows.DataObject dragOverFiles = (System.Windows.DataObject)dropInfo.Data;
            if (dragOverFiles == null)
            {
                Logger.Fatal(CLASS_NAME, "ConvertDropInfoToPathList", "dragOverFiles are null!");
                return false;
            }

            System.Collections.Specialized.StringCollection dragOverFileList = dragOverFiles.GetFileDropList();
            if (dragOverFileList == null || dragOverFileList.Count < 1)
            {
                Logger.Fatal(CLASS_NAME, "ConvertDropInfoToPathList", "dragOverFileList is invalid!");
                return false;
            }

            filePathList = dragOverFileList.Cast<string>().ToList();
            if (filePathList == null || filePathList.Any() == false)
            {
                Logger.Fatal(CLASS_NAME, "ConvertDropInfoToPathList", "filePathList is invalid!");
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
            // CanDropで高頻度呼ばれるため、基本的にログを出さない

            if (string.IsNullOrEmpty(filePath))
            {
                Logger.Fatal(CLASS_NAME, "IsTargetFile", "filePath is null or empty!");
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
            Logger.Info(CLASS_NAME, "SearchAllWordsInText", $"start. searchWord:[{searchWord}], margin:[{margin}]");

            // check args
            if (string.IsNullOrEmpty(searchWord))
            {
                Logger.Fatal(CLASS_NAME, "SearchAllWordsInText", "searchWord is null or empty!");
                return null;
            }
            else if (string.IsNullOrEmpty(targetText))
            {
                Logger.Fatal(CLASS_NAME, "SearchAllWordsInText", "targetText is null or empty!");
                return null;
            }
            else if (margin < 0 /*最大値は現状未定、最小値も設定ファイルや定数で外出しする予定だが、現状ハードコーティングとする*/)
            {
                Logger.Fatal(CLASS_NAME, "SearchAllWordsInText", $"margin is incorrect! value:[{margin}]");
                return null;
            }

            // 検索対象語句のインデックスを取得する
            int[] searchResultIndexArray = GetAllSearchResultIndex(searchWord, targetText);
            if (searchResultIndexArray == null)
            {
                // nullは異常な場合
                Logger.Fatal(CLASS_NAME, "SearchAllWordsInText", "searchResultIndexArray is null!");
                return null;
            }
            else if (searchResultIndexArray.Any() == false)
            {
                // 検索したが結果が無い場合はEmptyを返す
                Logger.Info(CLASS_NAME, "SearchAllWordsInText", "No search result.");
                return new Dictionary<int, string>();
            }
            int searchResultCount = searchResultIndexArray.Count();

            string[] searchResultWordArray = GetAllSearchResultWords(searchResultIndexArray, searchWord, targetText, margin);
            if (searchResultWordArray == null)
            {
                Logger.Fatal(CLASS_NAME, "SearchAllWordsInText", "searchResultWordsArray is null!");
                return null;
            }
            else if (searchResultWordArray.Any() == false)
            {
                // 検索したが結果が無い場合はEmptyを返す
                Logger.Error(CLASS_NAME, "SearchAllWordsInText", "searchResultWordsArray is empty!");
                return new Dictionary<int, string>();
            }

            // 最終的にDictionaryで返せばよくない？
            Dictionary<int/*index*/, string/*result*/> searchResultIndexWordPairs = new Dictionary<int, string>();
            for (int i = 0; i < searchResultCount; ++i)
            {
                searchResultIndexWordPairs.Add(searchResultIndexArray[i], searchResultWordArray[i]);
            }

            return searchResultIndexWordPairs;
        }

        /// <summary>
        /// 対象文章内における検索対象語句の全インデックスを取得する
        /// </summary>
        /// <param name="searchWord"></param>
        /// <param name="targetText"></param>
        /// <returns></returns>
        private int[] GetAllSearchResultIndex(string searchWord, string targetText)
        {
            if (string.IsNullOrEmpty(searchWord) || string.IsNullOrEmpty(targetText))
            {
                Logger.Fatal(CLASS_NAME, "GetAllSearchResultIndex", "args is null or empty!");
                return null;
            }

            // 文書中で該当するインデックスを一旦入れておくリストを用意
            List<int> searchResultIndexList = new List<int>();

            // 1箇所目をまず探す
            int foundIndex = targetText.IndexOf(searchWord);
            if (foundIndex < 0)
            {
                // 検索したが何もない場合はエラーではないので空の配列を戻すようにする
                return new int[0];
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
                searchResultIndexList.Add(foundIndex);

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

            return searchResultIndexList.ToArray();
        }

        /// <summary>先頭から順に対象語句を検索し、マージンを考慮した全検索結果を取得する</summary>
        /// <param name="allIndexArray"></param>
        /// <param name="searchWord"></param>
        /// <param name="targetText"></param>
        /// <returns></returns>
        private string[] GetAllSearchResultWords(int[] allIndexArray, string searchWord, string targetText, int margin)
        {
            #region check args

            if (allIndexArray == null || allIndexArray.Any() == false)
            {
                return null;
            }
            else if (string.IsNullOrEmpty(searchWord) || string.IsNullOrEmpty(targetText))
            {
                return null;
            }
            else if (margin < 0)
            {
                return null;
            }

            #endregion

            // 実際にテキストから、Viewに表示対象となる語句領域を切り取っていく
            // インデックス分だけ必ずあるはず
            int searchResultCount = allIndexArray.Count();
            string[] searchResultWordArray = new string[searchResultCount];
            for (int targetIndex = 0; targetIndex < searchResultCount; ++targetIndex)
            {
                // 手前側マージン
                int frontMargin = allIndexArray[targetIndex] - margin;
                // 後ろ側マージン→インデックス＋検索対象語句＋マージン
                int behindMargin = allIndexArray[targetIndex] + searchWord.Length + margin;

                // 後ろのマージンがなくても、最後の検索とは限らないので、foreachは続けること
                // 例：「あああああああ」で「あ」だけを検索した場合
                if (frontMargin < 0 && targetText.Length < behindMargin + 1) // LengthとIndexを比較するのでIndexに+1しておく
                {
                    // 手前に規定値分のマージンがなく、後ろにも規定値分のマージンがない場合
                    // 「文字列の最初～文字列の最後」までを切り取る→検索対象の文字列をそのまま入れ込む
                    searchResultWordArray[targetIndex] = targetText;
                }
                else if (frontMargin < 0)
                {
                    // 手前に規定値分のマージンがなく、後ろには規定値分のマージンがある場合
                    // 「文字列の最初～インデックス＋検索対象語句＋後ろのマージン」だけ切り取る
                    searchResultWordArray[targetIndex] = targetText.Substring(0, searchWord.Length + margin); // substringの第2引数は切り取る文字数
                }
                else if (targetText.Length < behindMargin + 1)
                {
                    // 手前に規定値分のマージンがあり、後ろには規定値分のマージンがない場合
                    // 「手前のマージン～文字列の最後」までを切り取る
                    searchResultWordArray[targetIndex] = targetText.Substring(frontMargin);

                }
                else
                {
                    // 手前に規定値分のマージンがあり、後ろにも規定値分のマージンがある場合
                    // 「手前のマージン～インデックス＋検索対象語句＋後ろのマージン」だけ切り取る
                    // marginを2倍しておかないと手前のmargin分しか切り取れない
                    searchResultWordArray[targetIndex] = targetText.Substring(frontMargin, searchWord.Length + 2 * margin);
                }
            }

            return searchResultWordArray;
        }

        internal void OpenSettingsWindow()
        {
            throw new NotImplementedException();
        }

        /// <summary>ファイルを開くダイアログを表示し、既存のファイルを読み込みます</summary>
        internal void OpenFile()
        {
            // 現在表示中のテキストが編集済みか否かを判定する

            // 保存されていなければ、Yes/Noダイアログを出して確認する

            // 破棄OKか、保存済みであれば現在表示中のテキストとXshdをクリアする

            // ファイルを開く

            // MainWindowのAvalonEditに適用する
        }

        /// <summary>テキストファイルを新規作成します</summary>
        /// <remarks>true:正常, false:異常</remarks>
        internal bool CreateNewFile()
        {
            // 現在表示中のテキストが編集済みか否かを判定する
            if (IsModifiedOrNewFile)
            {
                if(_dialogManager==null)
                {
                    Logger.Fatal(CLASS_NAME, "CreateNewFile", "_dialogManager is null!");
                    return false;
                }

                // 保存されていなければ、Yes/Noダイアログを出して確認する
                DialogResult dialogResult = DialogResult.Cancel;
                bool result = _dialogManager.OpenOkCancelDialog("現在編集中の文章を破棄しますか？", out dialogResult);
                if (result == false)
                {
                    Logger.Fatal(CLASS_NAME, "CreateNewFile", $"Dialog error! dialogResult:[{dialogResult}]");
                    return false;
                }

                if (dialogResult == DialogResult.Cancel)
                {
                    Logger.Info(CLASS_NAME, "CreateNewFile", "Canceled discard text and create new file");
                    return true;
                }
            }

            // 破棄OKか、保存済みであれば現在表示中のテキストとXshdをクリアする
            TextEditor.Text = string.Empty;
            _highlightManager.ResetHighlightInfo();

            // ファイル保存ダイアログを表示する
            string saveFilePath = null;
            if (_dialogManager.OpenSaveAsDialog(out saveFilePath) == false)
            {
                Logger.Info(CLASS_NAME, "CreateNewFile", "create new file failed.");
                return false;
            }

            if (string.IsNullOrEmpty(saveFilePath))
            {
                Logger.Fatal(CLASS_NAME, "CreateNewFile", "Filename is null or empty!");
                return false;
            }

            // ファイルを保存する
            if(_fileAccessManager==null)
            {
                Logger.Error(CLASS_NAME, "CreateNewFile", "_fileAccessManager is null!");
                return false;
            }

            _fileAccessManager.SaveNewFile(saveFilePath);

            // 保存したファイルパスを保持する
            DisplayTextFilePath = saveFilePath;

            // 編集済みフラグを下げる
            IsModifiedOrNewFile = false;

            return true;
        }

        /// <summary>DBに登録されている全類語グループを取得する</summary>
        /// <returns>正常時：DBに登録されている全類語グループ、異常時:false</returns>
        internal CommonLibrary.Entity.SynonymGroupEntity[] GetAllSynonymGroups()
        {
            return Manager.SynonymManager.GetAllSynonymGroup();
        }

        /// <summary>類語グループIDに紐付く類語一覧を取得する</summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        internal CommonLibrary.Entity.SynonymWordEntity[] GetSynonymWordEntities(int groupId)
        {
            return Manager.SynonymManager.GetSynonymWordEntities(groupId);
        }

        /// <summary>類語検索処理を実施する</summary>
        /// <param name="groupId">選択中の類語グループID</param>
        /// <param name="targetText">対象（表示中）テキスト</param>
        /// <returns>結果配列</returns>
        internal MainWindowVM.DisplaySynonymSearchResult[] SynonymSearch(MainWindowVM.DisplaySynonymWord[] targetSynonyms, string targetText)
        {
            #region check args

            if (targetSynonyms == null || targetSynonyms.Any() == false)
            {
                Logger.Fatal(CLASS_NAME, "SynonymSearch", "targetSynonyms are null");
                return null;
            }

            if (string.IsNullOrEmpty(targetText))
            {
                Logger.Fatal(CLASS_NAME, "SynonymSearch", "targetText is null");
                return null;
            }

            #endregion

            Logger.Info(CLASS_NAME, "SynonymSearch", $"start. targetSynonyms count is {targetSynonyms.Count()}");

            // 類語の全検索結果を取得
            List<MainWindowVM.DisplaySynonymSearchResult> unsortedSynonymSearchResults
                = GetAllSynonymSearchResult(targetSynonyms, targetText);

            // Index順にSortして配列化する。昇順であることを保証したいが、動作が不安定になる場合は
            // 将来的にSortedDictionaryとOrderdDictionaryの使用を考える
            MainWindowVM.DisplaySynonymSearchResult[] sortedSynonymSearchResultArray
                = unsortedSynonymSearchResults.OrderBy(result => result.Index).ToArray();

            // 参照型を値渡しして、resultのRepeatCountとUsingCountを取得してreturnする
            // 参照型の参照渡しをするとインスタンスごと書き換えられるリスクがあるので許容しない
            AdjustRepeatCountAndUsingCount(sortedSynonymSearchResultArray);
            return sortedSynonymSearchResultArray;
        }

        /// <summary>渡された類語検索結果に基づいて、内部のRepeatCountとUsingCountを計算する</summary>
        /// <param name="sortedSynonymSearchResult">indexで昇順ソート済みの類語検索結果配列</param>
        /// <remarks>引数の連続した2要素を参照するため、ソート済みでないと結果がおかしくなる</remarks>
        private void AdjustRepeatCountAndUsingCount(MainWindowVM.DisplaySynonymSearchResult[] sortedSynonymSearchResult)
        {
            // 初回はRepeatCountもUsingCountも0確定のため、繰り返しのindexは1から開始する
            for (int index = 1; index < sortedSynonymSearchResult.Count(); ++index)
            {
                // アクセス負荷軽減のため、一旦ローカルに取り出す
                MainWindowVM.DisplaySynonymSearchResult indexEntity = sortedSynonymSearchResult[index];

                // 直前に存在しているか否か（RepeatCount）
                MainWindowVM.DisplaySynonymSearchResult preIndexEntity = sortedSynonymSearchResult[index - 1];
                if (indexEntity.SynonymWord == preIndexEntity.SynonymWord)
                {
                    // 直前にヒットしていた結果の類語が、現在の類語と同じであれば、繰り返し回数を+1する
                    indexEntity.RepeatCount = preIndexEntity.RepeatCount;
                    ++indexEntity.RepeatCount;
                }
                else
                {
                    // 直前にヒットしていた結果の類語が、現在の類語と異なるなら、繰り返し回数を0とする
                    indexEntity.RepeatCount = 0;
                }

                // これまでに何回ヒットしているか（UsingCount）
                // これまでに何個同じSynonymWordが存在しているかと同義になる
                int usingCount = sortedSynonymSearchResult.Count(
                    entity => entity != null &&                             // nullチェック
                              entity.Index < indexEntity.Index &&           // 現在の要素以前
                              entity.SynonymWord == indexEntity.SynonymWord // 現在の類語と同じである
                    );
                indexEntity.UsingCount = usingCount;
            }

            return;
        }

        /// <summary>渡された全類語を対象の文章内から検索する</summary>
        /// <param name="targetSynonymWords">検索対象の全類語</param>
        /// <param name="targetText">検索先の文章</param>
        /// <returns>正常時：検索結果、異常時：null</returns>
        private List<MainWindowVM.DisplaySynonymSearchResult> GetAllSynonymSearchResult(MainWindowVM.DisplaySynonymWord[] targetSynonymWords, string targetText)
        {
            // 結果返却用のListを用意
            List<MainWindowVM.DisplaySynonymSearchResult> synonymSearchResults
                = new List<MainWindowVM.DisplaySynonymSearchResult>();

            foreach (MainWindowVM.DisplaySynonymWord target in targetSynonymWords)
            {
                if (target == null)
                {
                    Logger.Error(CLASS_NAME, "GetAllSynonymSearchResult", "target is null!");
                    continue;
                }

                int[] allIndexinText = GetAllSearchResultIndex(target.SynonymWord, targetText);
                if (allIndexinText == null || allIndexinText.Any() == false)
                {
                    Logger.Fatal(CLASS_NAME, "GetAllSynonymSearchResult", "allIndexInText is null or empty!");
                    return null;
                }

                // todo:marginはハードコーディングになっているので、設定ファイルに外だしなどする
                string[] allResultinText = GetAllSearchResultWords(allIndexinText, target.SynonymWord, targetText, _viewModel.SEARCHRESULT_MARGIN);
                if (allResultinText == null || allResultinText.Any() == false)
                {
                    Logger.Fatal(CLASS_NAME, "GetAllSynonymSearchResult", "allResultinText is null or empty!");
                    return null;
                }

                // alIndexとallResultは先頭から順に対応しているはずなので、それをペアにしてDicに入れ込んでいく
                // 個数が異なったら何かがおかしい
                if (allIndexinText.Count() != allResultinText.Count())
                {
                    Logger.Fatal(CLASS_NAME, "GetAllSynonymSearchResult", $"index and result is incorrect. index:[{allIndexinText.Count()}], result[{allResultinText.Count()}]");
                    return null;
                }

                for (int i = 0; i < allResultinText.Count(); ++i)
                {
                    synonymSearchResults.Add(
                        new MainWindowVM.DisplaySynonymSearchResult()
                        {
                            SynonymWord = target.SynonymWord,
                            UsingSection = allResultinText[i],
                            Index = allIndexinText[i]
                        }
                        );
                }
            }
            return synonymSearchResults;
        }

        /// <summary>
        /// キャレットの移動を行う
        /// </summary>
        /// <param name="index">カーソル配置位置</param>
        /// <returns>true:正常、false:異常</returns>
        internal bool UpdateCaretOffset(int index)
        {
            if (index < 0)
            {
                Logger.Error(CLASS_NAME, "UpdateCaretOffset", $"index is incorrect. index:[{index}]");
                return false;
            }

            if(TextEditor==null)
            {
                Logger.Fatal(CLASS_NAME, "UpdateCaretOffset", "TextEditor is null!");
            }

            // キャレット更新
            TextEditor.CaretOffset = index;
            TextEditor.TextArea.Caret.BringCaretToView();

            // BeginInvokeしないとFocusしてくれない
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() => { TextEditor.Focus(); }));

            return true;
        }

        #endregion
    }
}
