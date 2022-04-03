using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GongSolutions.Wpf.DragDrop;
using SynonyMe.CommonLibrary.Log;

namespace SynonyMe.Model.MainWindow
{
    /// <summary>メイン画面のテキスト編集領域に関する情報と処理を有します</summary>
    internal class AvalonEditModel
    {
        /// <summary>ログ出力用クラス名</summary>
        private const string CLASS_NAME = "AvalonEditModel";

        /// <summary>管理元のModel</summary>
        private MainWindowModel _parent = null;

        /// <summary>コンストラクタ</summary>
        /// <param name="parent">呼び出し元Modelクラス</param>
        public AvalonEditModel(MainWindowModel parent)
        {
            _parent = parent;
        }

        /// <summary>対象ファイルがドロップ可能（AvalonEditで管理可能）かを判定します</summary>
        /// <param name="dropInfo"></param>
        /// <returns>true:ドロップ可, false:不可</returns>
        private bool CanDrop(IDropInfo dropInfo)
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
                if (string.IsNullOrEmpty(filePath))
                {
                    continue;
                }

                // 1つでも対象外のファイルがあれば弾く
                if (IsTargetFile(filePath) == false)
                {
                    // ドラッグオーバー中に大量のログが出るため、ログ出力は抑制する
                    // Logger.Info(CLASS_NAME, "CanDrop", $"there is not target file. file name is {filePath}");
                    dropInfo.Effects = System.Windows.DragDropEffects.None;
                    return false;
                }
            }

            // 全て問題なければドロップ可とする
            return true;
        }

        /// <summary>ドラッグオーバーされたファイル群について、ドロップ可不可を判定してマウスエフェクトを操作します</summary>
        internal void ChangeDragOverMouseEffect(IDropInfo dropInfo)
        {
            if (dropInfo == null)
            {
                Logger.Error(CLASS_NAME, "ChangeDragOverMouseEffect", "dropInfo is null!");
                return;
            }

            if (CanDrop(dropInfo))
            {
                dropInfo.Effects = System.Windows.DragDropEffects.Copy;
            }
            else
            {
                dropInfo.Effects = System.Windows.DragDropEffects.None;
            }
        }

        /// <summary>ドロップファイルを保持して表示します</summary>
        /// <param name="dropInfo"></param>
        internal void Drop(IDropInfo dropInfo)
        {
            if (CanDrop(dropInfo) == false)
            {
                Logger.Fatal(CLASS_NAME, "Drop", "CanDrop return false");
                return;
            }

            if (_parent == null)
            {
                Logger.Fatal(CLASS_NAME, "Drop", "_parent is null!");
                return;
            }

            if (_parent.ViewModel == null)
            {
                Logger.Fatal(CLASS_NAME, "Drop", "ViewModel is null!");
                return;
            }

            if (_parent.ViewModel.OpeningFiles == null)
            {
                Logger.Fatal(CLASS_NAME, "Drop", "OpeningFiles are null!");
                return;
            }

            // todo:現在は強制的に破棄している
            _parent.ViewModel.OpeningFiles.Clear();

            // 将来的にはタブを分離させる必要があるので、そのための仮処置
            List<string> displayTargetFilePaths = GetDisplayTextFilePath(dropInfo);
            foreach (string filePath in displayTargetFilePaths)
            {
                _parent.ViewModel.OpeningFiles.Add(_parent.ViewModel.TabId, filePath);
                //++_tabId; // TabIdは現状インクリメントしない（0番目を前提として扱っているので
            }

            // 現状、表示可能テキストは1つだけなので、0番目を使用する
            // 対象の全ファイルを開き、内部で保持する(現状、1つのファイルしか開けないが引数は複数に対応させるだけさせておく)
            SetTextDocuments(_parent.ViewModel.OpeningFiles);
        }


        /// <summary>表示対象ファイルのパス(絶対パス)を取得する</summary>
        /// <param name="dropInfo">ドロップされたファイル情報</param>
        /// <returns>対象ファイルの絶対パス</returns>
        private List<string> GetDisplayTextFilePath(IDropInfo dropInfo)
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

        /// <summary>依頼された全ファイルをTextEditorで管理させる</summary>
        /// <param name="openingFiles"></param>
        internal void SetTextDocuments(Dictionary<int, string> openingFiles)
        {
            if (openingFiles == null)
            {
                Logger.Error(CLASS_NAME, "SetTextDocuments", "openingFiles are null or empty!");
                return;
            }

            // todo:複数タブに対応
            _parent.DisplayTextFilePath = openingFiles[0];
            string text = FileAccessor.GetFileAccessor.LoadTextFile(_parent.DisplayTextFilePath);
            _parent.ViewModel.TextDocument.Text = text;

            // ドロップ直後に「編集済み」が出るのを抑制する
            _parent.ViewModel.IsModified = false;//todo:check null

            // Ctrl + Sで名前をつけて保存にしなくて良くする
            _parent.ForceSaveAs = false;
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

            System.Windows.DataObject dragOverFiles = dropInfo.Data as System.Windows.DataObject;
            if (dragOverFiles == null)
            {
                Logger.Fatal(CLASS_NAME, "ConvertDropInfoToPathList", "dragOverFiles are null!");
                return false;
            }

            System.Collections.Specialized.StringCollection dragOverFileList = dragOverFiles.GetFileDropList();
            if (dragOverFileList == null || dragOverFileList.Count < 1)
            {
                Logger.Error(CLASS_NAME, "ConvertDropInfoToPathList", "dragOverFileList is invalid!");
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

            if (_parent.PROCESS_TARGET_FILE_EXTENSIONS.Contains(extension))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
