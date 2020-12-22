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
            if(string.IsNullOrEmpty(filePath) ||
               string.IsNullOrEmpty(displayText))
            {
                return false;
            }

            TextEditor editor = new TextEditor();
            try
            {
                editor.Load(filePath);
                editor.Text = displayText;
                editor.Save(filePath);
            }
            catch(Exception e)
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
            if(filePathList == null || filePathList.Any()==false)
            {
                return null;
            }

            List<string> textList = new List<string>();
            foreach(string filePath in filePathList)
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    continue;
                }

                string text = null;
                if(Load(filePath, out text))
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
            if(string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            TextEditor textEditor = new TextEditor();
            try
            {
                textEditor.Load(filePath);
            }
            catch(Exception e)
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

        #endregion
    }
}
