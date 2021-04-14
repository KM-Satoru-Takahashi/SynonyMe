﻿using System;
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
using SynonyMe.CommonLibrary.Entity;
using ICSharpCode.AvalonEdit.Document;
using System.ComponentModel;

namespace SynonyMe.ViewModel
{
    public class MainWindowVM : ViewModelBase, IDropTarget
    {
        #region field

        /// <summary>Model</summary>
        private SynonyMe.Model.MainWindowModel _model = null;

        /// <summary>画面表示中テキストの絶対パス</summary>
        /// 将来的にタブVMへ移管予定
        private string _displayTextFilePath = null;

        /// <summary>開いているファイル情報</summary>
        /// <remarks>将来、タブで同時に複数ファイルを開くことを考えてDictionaryで管理する</remarks>
        private Dictionary<int, string> _openingFiles = new Dictionary<int/*タブID*/, string/*ファイルパス*/>();

        /// <summary>検索結果リスト</summary>
        private ObservableCollection<SearchResultEntity> _searchResult = new ObservableCollection<SearchResultEntity>();

        /// <summary>検索結果リストの表示状態</summary>
        private Visibility _searchResultVisibility = Visibility.Hidden;

        /// <summary>単語検索時、前後何文字を検索結果として表示するか</summary>
        /// <remarks>将来的にはユーザが設定変更可能にするが、試作段階では前後10文字固定とする</remarks>
        private int SEARCHRESULT_MARGIN = 10;

        private string _wordCount = null;

        private string _numberOfLines = null;

        private Visibility _editedTextVisible = Visibility.Hidden;

        private TextEditor _textEditor = new TextEditor();

        /// <summary>AvalonEditの文章管理インスタンス</summary>
        private TextDocument _displayTextDoc = null;

        #endregion

        #region property

        internal TextEditor TextEditor
        {
            get
            {
                return _textEditor;
            }
        }

        /// <summary>ウィンドウタイトル</summary>
        public string MainWindowTitle { get; } = "SynonyMe";

        /// <summary>ツールバー部分の高さ(固定値)</summary>
        public int ToolbarHeight { get; } = 40;

        /// <summary>フッター部分の高さ(固定値)</summary>
        public int FooterHeight { get; } = 30;

        /// <summary>検索ボタン表示文字列</summary>
        public string SearchButtonText { get; } = "検索";

        /// <summary>ドラッグアンドドロップで文章を表示する領域</summary>
        public TextDocument DisplayTextDoc
        {
            get { return _displayTextDoc; }
            set { _displayTextDoc = value; OnPropertyChanged("DisplayTextDoc"); }
        }

        /// <summary>検索文字列</summary>
        public string SearchWord { get; set; } = null;

        /// <summary>検索結果</summary>
        public ObservableCollection<SearchResultEntity> SearchResult
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

        public string WordCountText { get; } = "文字数：";
        public string WordCount
        {
            get
            {
                return _wordCount;
            }
            set
            {
                if (_wordCount == value)
                {
                    return;
                }

                _wordCount = value;
                OnPropertyChanged("WordCount");
            }
        }

        public string NumberOfLinesText { get; } = "行数：";
        public string NumberOfLines
        {
            get
            {
                return _numberOfLines;
            }
            set
            {
                if (_numberOfLines == value)
                {
                    return;
                }

                _numberOfLines = value;
                OnPropertyChanged("NumberOfLines");
            }
        }


        public string EditedText { get; } = "編集済み";

        public Visibility EditedTextVisible
        {
            get
            {
                return _editedTextVisible;
            }
            private set
            {
                if (_editedTextVisible == value)
                {
                    return;
                }

                _editedTextVisible = value;
                OnPropertyChanged("EditedTextVisible");
            }
        }

        #region command

        /// <summary>保存ボタン</summary>
        public ICommand Command_Save { get; private set; } = null;

        /// <summary>類語コマンド</summary>
        public ICommand Command_OpenSynonymWindow { get; private set; } = null;

        /// <summary>検索コマンド</summary>
        public ICommand Command_Search { get; private set; } = null;

        /// <summary>検索結果クリック時のジャンプコマンド</summary>
        public ICommand Command_JumpToSearchResult { get; private set; } = null;

        /// <summary>文字数等の更新処理</summary>
        public ICommand Command_UpdateTextInfo { get; private set; } = null;

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

            _displayTextDoc = _textEditor.Document;

            // コマンド初期化処理
            Command_Save = new CommandBase(ExecuteSave, null);
            Command_OpenSynonymWindow = new CommandBase(ExecuteOpenSynonymWindow, null);
            Command_Search = new CommandBase(ExecuteSearch, null);
            Command_JumpToSearchResult = new CommandBase(ExecuteJumpToSearchResult, null);
            Command_UpdateTextInfo = new CommandBase(ExecuteUpdateTextInfo, null);

            // IsModifiedは通知タイミングがTextChangedより遅れるので、DependencyPropertyに登録しないと一歩遅れた処理になってしまう
            // 具体的には、最初の1回目のキーダウン（文字入力）を取得できない
            // TODO:DependencyPropertyDescriptorは強参照のためメモリリークの恐れがあり、要調査
            var descripter = DependencyPropertyDescriptor.FromProperty(TextEditor.IsModifiedProperty, typeof(TextEditor));
            if (descripter != null)
            {
                descripter.RemoveValueChanged(TextEditor, OnIsModifiedChanged);
                descripter.AddValueChanged(TextEditor, OnIsModifiedChanged);
            }
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
            DisplayTextDoc.Text = _model.GetDisplayText(dropInfo)[0];
        }

        /// <summary>編集中のテキスト保存処理</summary>
        /// <param name="parameter"></param>
        private void ExecuteSave(object parameter)
        {
            if (_model == null)
            {
                throw new NullReferenceException("ExecuteSave _model is null");
            }

            _model.Save(_displayTextFilePath, DisplayTextDoc.Text);
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
            Dictionary<int, string> indexWordPairs = _model.SearchAllWordsInText(SearchWord, DisplayTextDoc.Text, SEARCHRESULT_MARGIN);
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

            SearchResultEntity[] searchResults = new SearchResultEntity[indexWordPairs.Count];
            int index = 0;
            foreach (KeyValuePair<int, string> kvp in indexWordPairs)
            {
                searchResults[index] = new SearchResultEntity()
                {
                    Index = kvp.Key,
                    DisplayWord = kvp.Value
                };
                ++index;
            }

            SearchResult = new ObservableCollection<SearchResultEntity>(searchResults);
        }

        /// <summary>検索結果へのジャンプ処理</summary>
        /// <param name="parameter"></param>
        private void ExecuteJumpToSearchResult(object parameter)
        {
            SearchResultEntity searchResultEntity = parameter as SearchResultEntity;
            if (searchResultEntity == null)
            {
                return;
            }
        }

        /// <summary>画面上部のテキスト情報更新処理</summary>
        /// <param name="parameter"></param>
        private void ExecuteUpdateTextInfo(object parameter)
        {
            if (_displayTextDoc == null)
            {
                WordCount = null;
                NumberOfLines = null;
            }
            else
            {
                WordCount = _displayTextDoc.TextLength.ToString();
                NumberOfLines = _displayTextDoc.LineCount.ToString();
            }
        }

        /// <summary>編集済み判定処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnIsModifiedChanged(object sender, EventArgs e)
        {
            TextEditor textEditor = sender as TextEditor;
            if (textEditor == null)
            {
                return;
            }

            if (textEditor.IsModified)
            {
                EditedTextVisible = Visibility.Visible;
            }
            else
            {
                EditedTextVisible = Visibility.Hidden;
            }
        }


        #endregion
    }
}
