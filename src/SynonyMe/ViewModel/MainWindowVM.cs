using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.ObjectModel;
using System.IO;
using GongSolutions.Wpf.DragDrop;
using ICSharpCode.AvalonEdit;

namespace SynonyMe.ViewModel
{
    public class MainWindowVM : ViewModelBase, IDropTarget
    {
        #region field

        /// <summary>本プロセスで処理対象となるファイル拡張子一覧</summary>
        /// <remarks>ここに含まれない拡張子のファイルは読み込み時に弾かれる</remarks>
        private static readonly string[] PROCESS_TARGET_FILE_EXTENSIONS = new string[]
        {
            ".txt"
        };

        private SynonyMe.Model.MainWindowModel _model = null;

        private string _displayText = null;

        #endregion

        #region property

        public string MainWindowTitle { get; } = "SynonyMe";

        public int ToolbarHeight { get; } = 20;

        public int FooterHeight { get; } = 30;

        public string DisplayText
        {
            get
            {
                return _displayText;
            }
            set
            {
                _displayText = value;
                RaisePropertyChanged("DisplayText");
            }
        }

        #endregion

        #region method

        /// <summary>コンストラクタ</summary>
        public MainWindowVM()
        {
            Initialize();
        }

        /// <summary>modelの生成等の初期化処理を実施</summary>
        private void Initialize()
        {
            _model = new Model.MainWindowModel(this);
        }

        /// <summary>ドラッグオーバー時(マウスをドラッグで重ねた際)に対象ファイルでなければ弾く</summary>
        /// <param name="dropInfo">ドラッグオーバーされているファイル情報</param>
        /// <remarks>テキストファイル以外であれば</remarks>
        public void DragOver(IDropInfo dropInfo)
        {
            var dragOverFilePathList = Enumerable.Empty<string>();

            if (ConvertDropInfoToPathList(dropInfo, out dragOverFilePathList) == false)
            {
                return;
            }

            if (dragOverFilePathList == null || dragOverFilePathList.Any() == false)
            {
                return;
            }

            foreach (string filePath in dragOverFilePathList)
            {
                // 1つでも対象外のファイルがあれば弾く
                if (IsTargetFile(filePath) == false)
                {
                    dropInfo.Effects = DragDropEffects.None;
                }
            }

            // 何事もなければドロップ可とする
            dropInfo.Effects = DragDropEffects.Copy;
        }

        /// <summary>渡されたファイルパスが処理対象のファイルかどうかを判定する</summary>
        /// <param name="filePath"></param>
        /// <returns>true:処理対象のファイル, false:それ以外のファイル</returns>
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

        /// <summary>ドロップされたテキストファイルを画面に表示する</summary>
        /// <param name="dropInfo"></param>
        /// <remarks>対応しない拡張子はDragOverで弾いている前提だが、チェックした方が良い</remarks>
        public void Drop(IDropInfo dropInfo)
        {
            var dragOverFilePathList = Enumerable.Empty<string>();

            if (ConvertDropInfoToPathList(dropInfo, out dragOverFilePathList) == false)
            {
                return;
            }

            if (dragOverFilePathList == null || dragOverFilePathList.Any() == false)
            {
                return;
            }

            foreach (string filePath in dragOverFilePathList)
            {
                if (IsTargetFile(filePath))
                {
                    TextEditor editor = new TextEditor();
                    editor.Load(filePath);
                    DisplayText = editor.Text;
                }
            }
        }

        /// <summary>DropInfoをファイルパスのリストに変換する</summary>
        /// <param name="dropInfo"></param>
        /// <param name="filePathList"></param>
        /// <returns>true:成功, false:失敗/異常</returns>
        private bool ConvertDropInfoToPathList(IDropInfo dropInfo, out IEnumerable<string> filePathList)
        {
            filePathList = Enumerable.Empty<string>();

            if (dropInfo == null)
            {
                return false;
            }

            DataObject dragOverFiles = (DataObject)dropInfo.Data;
            if (dragOverFiles == null)
            {
                return false;
            }

            var dragOverFileList = dragOverFiles.GetFileDropList();
            if (dragOverFileList == null || dragOverFileList.Count < 1)
            {
                return false;
            }

            filePathList = dragOverFileList.Cast<string>();
            if (filePathList == null || filePathList.Any() == false)
            {
                return false;
            }

            return true;
        }


        #endregion
    }
}
