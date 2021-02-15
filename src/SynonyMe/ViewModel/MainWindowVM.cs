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
using System.Windows.Input;

namespace SynonyMe.ViewModel
{
    public class MainWindowVM : ViewModelBase, IDropTarget
    {
        #region field

        /// <summary>Model</summary>
        private SynonyMe.Model.MainWindowModel _model = null;

        /// <summary>画面表示テキスト</summary>
        /// 将来的にタブVMへ移管予定
        private string _displayText = null;

        /// <summary>画面表示中テキストの絶対パス</summary>
        /// 将来的にタブVMへ移管予定
        private string _displayTextFilePath = null;

        /// <summary>開いているファイル情報</summary>
        /// <remarks>将来、タブで同時に複数ファイルを開くことを考えてDictionaryで管理する</remarks>
        private Dictionary<int, string> _openingFiles = new Dictionary<int/*タブID*/, string/*ファイルパス*/>();

        #endregion

        #region property

        /// <summary>ウィンドウタイトル</summary>
        public string MainWindowTitle { get; } = "SynonyMe";

        /// <summary>ツールバー部分の高さ(固定値)</summary>
        public int ToolbarHeight { get; } = 40;

        /// <summary>フッター部分の高さ(固定値)</summary>
        public int FooterHeight { get; } = 30;

        /// <summary>文章表示領域の表示テキスト</summary>
        public string DisplayText
        {
            get
            {
                return _displayText;
            }
            set
            {
                _displayText = value;
                OnPropertyChanged("DisplayText");
            }
        }

        #region command

        /// <summary>保存ボタン</summary>
        public ICommand Command_Save { get; protected set; } = null;

        /// <summary>類語コマンド</summary>
        public ICommand Command_OpenSynonymWindow { get; protected set; } = null;

        #endregion

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

            // コマンド初期化処理
            Command_Save = new CommandBase(ExecuteSave, null);
            Command_OpenSynonymWindow = new CommandBase(ExecuteOpenSynonymWindow, null);
        }

        /// <summary>ドラッグオーバー時(マウスをドラッグで重ねた際)に対象ファイルでなければ弾く</summary>
        /// <param name="dropInfo">ドラッグオーバーされているファイル情報</param>
        /// <remarks>テキストファイル以外であればNG</remarks>
        public void DragOver(IDropInfo dropInfo)
        {
            if (_model == null)
            {
                throw new NullReferenceException();
            }

            if (_model.CanDrop(dropInfo))
            {
                dropInfo.Effects = DragDropEffects.Copy;
            }
            else
            {
                dropInfo.Effects = DragDropEffects.None;
            }
        }

        /// <summary>ドロップされたテキストファイルを画面に表示する</summary>
        /// <param name="dropInfo">ドロップされたファイル群</param>
        /// <remarks>対応しない拡張子はDragOverで弾いている前提だが、チェックした方が良い</remarks>
        public void Drop(IDropInfo dropInfo)
        {
            if (_model == null)
            {
                throw new NullReferenceException();
            }

            if (_model.CanDrop(dropInfo) == false)
            {
                return;
            }

            // 将来的にはタブを分離させる必要があるので、そのための仮処置
            List<string> displayTargetFilePaths = _model.GetDisplayTextFilePath(dropInfo);

            // 現状、表示可能テキストは1つだけなので、0番目を使用する
            _displayTextFilePath = displayTargetFilePaths[0];
            DisplayText = _model.GetDisplayText(dropInfo)[0];
        }

        private void ExecuteSave(object parameter)
        {
            _model.Save(_displayTextFilePath, _displayText);
        }

        private void ExecuteOpenSynonymWindow(object parameter)
        {
            _model.OpenSynonymWindow();
        }

        #endregion
    }
}
