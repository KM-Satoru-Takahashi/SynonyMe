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

        /// <summary>検索結果リスト</summary>
        private ObservableCollection<string> _searchResult = new ObservableCollection<string>();

        /// <summary>検索結果リストの表示状態</summary>
        private Visibility _searchResultVisibility = Visibility.Hidden;

        /// <summary>単語検索時、前後何文字を検索結果として表示するか</summary>
        /// <remarks>将来的にはユーザが設定変更可能にするが、試作段階では前後10文字固定とする</remarks>
        private int SEARCHRESULT_MARGIN = 10;

        #endregion

        #region property

        /// <summary>ウィンドウタイトル</summary>
        public string MainWindowTitle { get; } = "SynonyMe";

        /// <summary>ツールバー部分の高さ(固定値)</summary>
        public int ToolbarHeight { get; } = 40;

        /// <summary>フッター部分の高さ(固定値)</summary>
        public int FooterHeight { get; } = 30;

        /// <summary>検索ボタン表示文字列</summary>
        public string SearchButtonText { get; } = "検索";

        /// <summary>文章表示領域の表示テキスト</summary>
        public string DisplayText
        {
            get
            {
                return _displayText;
            }
            set
            {
                if (_displayText == value)
                {
                    return;
                }

                _displayText = value;
                OnPropertyChanged("DisplayText");
            }
        }

        /// <summary>検索文字列</summary>
        public string SearchWord { get; set; } = null;

        /// <summary>検索結果</summary>
        public ObservableCollection<string> SearchResult
        {
            get
            {
                return _searchResult;
            }
            set
            {
                if (_searchResult == value)
                {
                    return;
                }

                _searchResult = value;
                OnPropertyChanged("SearchResult");
            }
        }

        /// <summary>検索結果表示状態</summary>
        public Visibility SearchResultVisibility
        {
            get
            {
                return _searchResultVisibility;
            }
            set
            {
                if (_searchResultVisibility == value)
                {
                    return;
                }

                _searchResultVisibility = value;
                OnPropertyChanged("SearchResultVisibility");
            }
        }

        #region command

        /// <summary>保存ボタン</summary>
        public ICommand Command_Save { get; private set; } = null;

        /// <summary>類語コマンド</summary>
        public ICommand Command_OpenSynonymWindow { get; private set; } = null;

        /// <summary>検索コマンド</summary>
        public ICommand Command_Search { get; private set; } = null;

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
            Command_Search = new CommandBase(ExecuteSearch, null);
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

        /// <summary>編集中のテキスト保存処理</summary>
        /// <param name="parameter"></param>
        private void ExecuteSave(object parameter)
        {
            if (_model == null)
            {
                throw new NullReferenceException("ExecuteSave _model is null");
            }

            _model.Save(_displayTextFilePath, _displayText);
        }

        /// <summary>類語ウィンドウを開く</summary>
        /// <param name="parameter"></param>
        private void ExecuteOpenSynonymWindow(object parameter)
        {
            if (_model == null)
            {
                throw new NullReferenceException("ExecuteOpenSynonymWindow _model is null");
            }

            _model.OpenSynonymWindow();
        }

        /// <summary>検索処理</summary>
        /// <param name="parameter"></param>
        private void ExecuteSearch(object parameter)
        {
            if (_model == null)
            {
                throw new NullReferenceException("ExecuteSearch _model is null");
            }

            if (string.IsNullOrEmpty(SearchWord))
            {
                return;
            }

            // dicのintはindex部分なので本文ハイライト、stringは結果表示リストに使用する
            Dictionary<int, string> indexWordPairs = _model.SearchAllWordsInText(SearchWord, DisplayText, SEARCHRESULT_MARGIN);
            if (indexWordPairs == null)
            {
                // nullなら表示を隠す
                SearchResultVisibility = Visibility.Hidden;
            }
            else if (indexWordPairs.Count < 1)
            {
                // 検索結果がなければ、その旨を表示する
            }
            else
            {
                // 検索結果ありの場合、結果を表示できるようにする
                SearchResultVisibility = Visibility.Visible;
            }


            // 念のため昇順にソートしておく
            indexWordPairs.OrderBy(pair => pair.Key);

            // 現状、indexは使用していないので、stringを順に取り出して利用する
            string[] searchResult = new string[indexWordPairs.Count];
            int index = 0;
            foreach (KeyValuePair<int, string> kvp in indexWordPairs)
            {
                searchResult[index] = kvp.Value;
                ++index;
            }

            SearchResult = new ObservableCollection<string>(searchResult);
        }

        #endregion
    }
}
