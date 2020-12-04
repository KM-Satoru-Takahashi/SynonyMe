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
        /// <remarks>初期値nullはよからぬことが起きそうなので空文字にする</remarks>
        private string _displayText = string.Empty;

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

        public ICommand Command_Save { get; protected set; } = null;

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

            if (_model.CanDrop(dropInfo))
            {
                DisplayText = _model.GetDisplayText(dropInfo);
            }
        }


        private void ExecuteSave(object parameter)
        {

        }

        #endregion
    }
}
