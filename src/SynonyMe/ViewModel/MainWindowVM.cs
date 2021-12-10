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
using SynonyMe.CommonLibrary.Entity;
using ICSharpCode.AvalonEdit.Document;
using System.ComponentModel;
using System.Windows.Threading;
using System.Windows.Controls;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System.Windows.Media;
using SynonyMe.CommonLibrary.Log;
using SynonyMe.CommonLibrary;

namespace SynonyMe.ViewModel
{
    public class MainWindowVM : ViewModelBase, IDropTarget
    {
        #region field

        /// <summary>Model</summary>
        private Model.MainWindowModel _model = null;

        /// <summary>開いているファイル情報</summary>
        /// <remarks>将来、タブで同時に複数ファイルを開くことを考えてDictionaryで管理する</remarks>
        private Dictionary<int, string> _openingFiles = new Dictionary<int/*タブID*/, string/*ファイルパス*/>();

        /// <summary>タブの並び替えとかもあるだろうが、とりあえずこうやって管理しておくことにする</summary>
        private int _tabId = 0;

        /// <summary>検索結果リストの表示状態</summary>
        private Visibility _searchResultVisibility = Visibility.Hidden;

        /// <summary>検索結果無し時の表示状態</summary>
        private Visibility _noSearchResultVisibility = Visibility.Hidden;

        /// <summary>単語検索時、前後何文字を検索結果として表示するか</summary>
        /// <remarks>将来的にはユーザが設定変更可能にするが、試作段階では前後10文字固定とする</remarks>
        internal int SEARCHRESULT_MARGIN = 10;

        /// <summary>文字数表示</summary>
        private string _wordCount = null;

        /// <summary>行数表示</summary>
        private string _numberOfLines = null;

        /// <summary>「編集済み」文字のVisibility</summary>
        private Visibility _editedTextVisible = Visibility.Hidden;

        /// <summary>現在表示中の類語グループID</summary>
        private int _selectedSynonymGroupId = -1;


        private const string CLASS_NAME = "MainWindowVM";

        #endregion

        #region property

        /// <summary>ウィンドウタイトル</summary>
        public string MainWindowTitle { get; } = CommonLibrary.MessageLibrary.MainWindowTitle;

        /// <summary>ツールバー部分の高さ(固定値)</summary>
        public int ToolbarHeight { get; } = 40;

        /// <summary>フッター部分の高さ(固定値)</summary>
        public int FooterHeight { get; } = 30;

        /// <summary>検索ボタン表示文字列</summary>
        public string SearchButtonText { get; } = MessageLibrary.SearchButtonText;

        /// <summary>類語検索ボタン表示文字列</summary>
        public string SearchSynonymText { get; } = CommonLibrary.MessageLibrary.SearchSynonymButtonText;

        /// <summary類語グループリストの類語名ヘッダ</summary>
        public string SynonymGroupNameHeader { get; } = CommonLibrary.MessageLibrary.MainWindowSynonymGroupName;

        /// <summary>類語グループリストの最終更新日ヘッダ</summary>
        public string SynonymGroupLastUpdateHeader { get; } = CommonLibrary.MessageLibrary.MainWindowSynonymGroupLastUpdate;

        /// <summary>類語リストの類語名ヘッダ</summary>
        public string SynonymWordHeader { get; } = CommonLibrary.MessageLibrary.MainWindowSynonymWordHeader;

        /// <summary>類語リストの連続使用(繰り返し)回数ヘッダ</summary>
        public string SynonymWordRepeatingCountHeader { get; } = CommonLibrary.MessageLibrary.MainWindowSynonymWordRepeatCountHeader;

        /// <summary>類語リストの合計使用回数ヘッダ</summary>
        public string SynonymWordUsingCountHeader { get; } = CommonLibrary.MessageLibrary.MainWindowSynonymWordUsingCountHeader;

        /// <summary>類語リストの使用箇所ヘッダ</summary>
        public string SynonymWordSectionHeader { get; } = CommonLibrary.MessageLibrary.MainWindowSynonymWordSectionHeader;

        /// <summary>ドラッグアンドドロップで文章を表示する領域</summary>
        public TextDocument DisplayTextDoc
        {
            get
            {
                if (_model != null)
                {
                    return _model.DisplayTextDocument;
                }

                // 異常系 Model側でログ出しているのでここではnullを返しておくだけでいい想定
                return null;
            }
            set
            {
                if (_model != null)
                {
                    _model.DisplayTextDocument = value;
                }
                OnPropertyChanged("DisplayTextDoc");
            }
        }

        /// <summary>検索文字列</summary>
        public string SearchWord { get; set; } = null;

        /// <summary>検索結果一覧</summary>
        /// <remarks>画面左側</remarks>
        public ObservableCollection<SearchResultEntity> SearchResult { get; set; } = new ObservableCollection<SearchResultEntity>();

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

        /// <summary>表示用の類語グループ一覧</summary>
        /// <remarks>画面右側上段</remarks>
        public ObservableCollection<SynonymGroupEntity> DisplaySynonymGroups { get; set; } = new ObservableCollection<SynonymGroupEntity>();

        /// <summary>類語一覧</summary>
        /// <remarks>画面右側中段</remarks>
        public ObservableCollection<DisplaySynonymWord> DisplaySynonymWords { get; set; } = new ObservableCollection<DisplaySynonymWord>();

        /// <summary>選択された類語グループと対応する、類語全件の検索結果</summary>
        /// <remarks>画面右側下段</remarks>
        public ObservableCollection<DisplaySynonymSearchResult> DisplaySynonymSearchResults { get; set; } = new ObservableCollection<DisplaySynonymSearchResult>();

        /// <summary>文字数表示の固定値「文字数」</summary>
        public string WordCountText { get; } = MessageLibrary.WordCountText;

        /// <summary>文字数表示箇所</summary>
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

        /// <summary>行数表示の固定値「行数」</summary>
        public string NumberOfLinesText { get; } = "行数：";

        /// <summary>行数表示箇所</summary>
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

        /// <summary>編集済みテキスト（固定値）</summary>
        public string EditedText { get; } = "編集済み";

        /// <summary>編集済みテキスト表示状態</summary>
        public Visibility EditedTextVisible
        {
            get
            {
                return _editedTextVisible;
            }
            set
            {
                if (_editedTextVisible == value)
                {
                    return;
                }

                _editedTextVisible = value;
                OnPropertyChanged("EditedTextVisible");
            }
        }

        /// <summary>検索結果が無い場合に表示される語句の表示状態</summary>
        public Visibility NoSearchResultVisibility
        {
            get
            {
                return _noSearchResultVisibility;
            }
            set
            {
                if (_noSearchResultVisibility == value)
                {
                    return;
                }

                _noSearchResultVisibility = value;
                OnPropertyChanged("NoSearchResultVisibility");
            }
        }

        /// <summary>テキスト表示領域の背景色</summary>
        public Brush AvalonEditBackGround { get; set; } = Brushes.White;

        /// <summary>検索結果がない場合に表示する文言</summary>
        /// <remarks>todo:CommonLibのDefineに移動させるべきでは？</remarks>
        public string NoSearchResultWord { get; } = "対象語句がありません";

        #region command

        #region toolbar

        /// <summary>新規作成ボタン</summary>
        public ICommand Command_CreateNewFile { get; private set; } = null;

        public string ToolTip_CreateNewFile { get; } = "新規作成\nCtrl+N";

        /// <summary>ファイル開くボタン</summary>
        public ICommand Command_OpenFile { get; private set; } = null;

        public string ToolTip_OpenFile { get; } = "開く\nCtrl+O";

        /// <summary>保存ボタン</summary>
        public ICommand Command_Save { get; private set; } = null;

        public string ToolTip_Save { get; } = "上書き保存\nCtrl+S";

        /// <summary>名前をつけて保存ボタン</summary>
        public ICommand Command_SaveAs { get; private set; } = null;

        public string ToolTip_SaveAs { get; } = "名前をつけて保存\nShift+Ctrl+S";

        /// <summary>類語コマンド</summary>
        public ICommand Command_OpenSynonymWindow { get; private set; } = null;

        public string ToolTip_OpenSynonymWindow { get; } = "類語設定\nAlt+S";

        /// <summary>設定画面コマンド</summary>
        public ICommand Command_OpenSettingsWindow { get; private set; } = null;

        public string ToolTip_OpenSettingsWindow { get; } = "設定\nAlt+O";

        #endregion


        /// <summary>検索コマンド</summary>
        public ICommand Command_Search { get; private set; } = null;

        /// <summary>検索結果クリック時のジャンプコマンド</summary>
        public ICommand Command_JumpToSearchResult { get; private set; } = null;

        /// <summary>文字数等の更新処理</summary>
        public ICommand Command_UpdateTextInfo { get; private set; } = null;

        /// <summary>類語グループ選択時の処理</summary>
        public ICommand Command_SelectSynonymGroup { get; private set; } = null;

        /// <summary>類語検索</summary>
        public ICommand Command_SynonymSearch { get; private set; } = null;

        /// <summary>類語検索結果クリック時のキャレット遷移処理</summary>
        public ICommand Command_JumpToSynonymSearchResult { get; private set; } = null;

        #endregion

        #endregion

        #region event

        /// <summary>類語グループあるいは類語一覧に更新があった際に発火するイベントハンドラ</summary>
        private event EventHandler UpdateSynonymEventHandler
        {
            add
            {
                if (_model != null)
                {
                    _model.UpdateSynonymEvent += value;
                }
            }
            remove
            {
                if (_model != null)
                {
                    _model.UpdateSynonymEvent -= value;
                }
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
            Logger.Info(CLASS_NAME, "Initialize", "start");

            _model = new Model.MainWindowModel(this);

            // コマンド初期化処理
            InitializeCommand();

            // 類語検索領域初期化処理
            InitializeSynonymSearch();

            // IsModifiedは通知タイミングがTextChangedより遅れるので、DependencyPropertyに登録しないと一歩遅れた処理になってしまう
            // 具体的には、最初の1回目のキーダウン（文字入力）を取得できない
            // DependencyPropertyDescriptorは強参照のため、参照を解除できず、繰り返し行うとメモリリークにつながる
            // 現状、Initializeは起動時にしか呼ばれず、動的にメインの文章表示領域が削除・再表示されることは現状ないので、一旦この実装で機能を満たす
            var descripter = DependencyPropertyDescriptor.FromProperty(TextEditor.IsModifiedProperty, typeof(TextEditor));
            if (descripter != null)
            {
                if (_model != null && _model.TextEditor != null)
                {
                    descripter.RemoveValueChanged(_model.TextEditor, OnIsModifiedChanged);
                    descripter.AddValueChanged(_model.TextEditor, OnIsModifiedChanged);
                }
                else
                {
                    Logger.Fatal(CLASS_NAME, "Initialize", "_model or TextEditor is null!");
                }
            }
        }

        /// <summary>各種コマンドを初期化します</summary>
        private void InitializeCommand()
        {
            #region toolbar

            Command_CreateNewFile = new CommandBase(ExecuteCreateNewFile, null);
            Command_OpenSettingsWindow = new CommandBase(ExecuteOpenSettingsWindow, null);
            Command_OpenFile = new CommandBase(ExecuteOpenFile, null);
            Command_SaveAs = new CommandBase(ExecuteSaveAs, null);
            Command_Save = new CommandBase(ExecuteSave, null);
            Command_OpenSynonymWindow = new CommandBase(ExecuteOpenSynonymWindow, null);

            #endregion

            Command_Search = new CommandBase(ExecuteSearch, null);
            Command_JumpToSearchResult = new CommandBase(ExecuteJumpToSearchResult, null);
            Command_JumpToSynonymSearchResult = new CommandBase(ExecuteJumpToSynonymSearchResult, null);
            Command_UpdateTextInfo = new CommandBase(ExecuteUpdateTextInfo, null);
            Command_SelectSynonymGroup = new CommandBase(ExecuteSelectSynonymGroup, null);
            Command_SynonymSearch = new CommandBase(ExecuteSynonymSearch, null);
        }

        #region Synonym method

        /// <summary>画面右側の類語検索領域にある類語グループと、紐付く類語リストを取得する</summary>
        private void InitializeSynonymSearch()
        {
            // 類語領域の表示を更新する
            UpdateSynonymArea(true);

            // 類語更新時のイベント登録
            // todo
            UpdateSynonymEventHandler -= UpdateSynonymSearchAreaEvent;
            UpdateSynonymEventHandler += UpdateSynonymSearchAreaEvent;
        }

        /// <summary>類語グループ一覧、類語一覧、類語検索結果を最新の状態に更新する</summary>
        private void UpdateSynonymArea(bool isInitialize = false)
        {
            UpdateDisplaySynonymGroups();

            // 類語グループを選択していて、当該グループが削除されずに存在している場合は更新
            if (IsExistSynonymGroupID(_selectedSynonymGroupId))
            {
                UpdateDisplaySynonymWords(_selectedSynonymGroupId);
            }
            else if (isInitialize == false)
            {
                // 類語グループを選択していないか、選択していたが削除されてなくなっている場合、クリア処理をかける
                // 初回起動時は選択していないこと確定なので、除外する
                DisplaySynonymWords.Clear();
            }

            // todo:タイミングどうするか
            // 類語検索結果は一旦クリアする
            DisplaySynonymSearchResults.Clear();
        }

        /// <summary>類語グループ一覧の表示を更新する</summary>
        private void UpdateDisplaySynonymGroups()
        {
            SynonymGroupEntity[] entities = _model.GetAllSynonymGroups();
            if (entities == null || entities.Any() == false)
            {
                DisplaySynonymGroups.Clear();
                return;
            }

            foreach (SynonymGroupEntity entity in entities)
            {
                // GroupIDはuniqueなので、重複していなければ追加する
                if (DisplaySynonymGroups.Any(synonymGroup => synonymGroup.GroupID == entity.GroupID))
                {
                    continue;
                }
                DisplaySynonymGroups.Add(entity);
            }
        }

        /// <summary>選択中の類語グループに基づき、類語一覧を更新する</summary>
        /// <param name="groupId">取得対象のグループID</param>
        private void UpdateDisplaySynonymWords(int groupId)
        {
            // 呼ばれるのは、類語ウィンドウで類語の更新がされたときか、メインウィンドウで類語グループ選択が変更されたとき
            // そのため、いずれにせよ一旦表示中の類語一覧をクリアしてしまった方が良い
            DisplaySynonymWords.Clear();

            if (_model == null)
            {
                return;
            }

            SynonymWordEntity[] entities = _model.GetSynonymWordEntities(groupId);
            if (entities == null || entities.Any() == false)
            {
                // todo:ログ
                Logger.Error(CLASS_NAME, "UpdateDisplaySynonymWords", "search synonym entites are null!");
            }

            // 表示用のEntityに入れ直して反映させる
            foreach (SynonymWordEntity entity in entities)
            {
                if (entity == null)
                {
                    continue;
                }

                DisplaySynonymWord displaySynonymWord = new DisplaySynonymWord()
                {
                    WordID = entity.WordID,
                    SynonymWord = entity.Word
                };
                DisplaySynonymWords.Add(displaySynonymWord);
            }
        }

        /// <summary>SynonymGroupsに引数のGroupIDが存在するかどうかを調べる</summary>
        /// <param name="groupId"></param>
        /// <returns>true:存在する、false:存在しない</returns>
        private bool IsExistSynonymGroupID(int groupId)
        {
            if (DisplaySynonymGroups == null ||
                DisplaySynonymGroups.Any(group => group.GroupID == groupId) == false)
            {
                return false;
            }
            return true;
        }

        /// <summary>SynonymWindowで類語グループ・類語一覧が更新された際に、MainWindowにも変更を反映させる</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateSynonymSearchAreaEvent(object sender, EventArgs e)
        {
            UpdateSynonymArea();
        }

        #endregion

        #region DragAndDrop

        /// <summary>ドラッグオーバー時(マウスをドラッグで重ねた際)に対象ファイルでなければ弾く</summary>
        /// <param name="dropInfo">ドラッグオーバーされているファイル情報</param>
        /// <remarks>テキストファイル以外であればNG</remarks>
        public void DragOver(IDropInfo dropInfo)
        {
            if (_model == null)
            {
                // ドラッグオーバー時に異常があった場合、握りつぶしてログ出しすると負荷がかかる
                // この後、他の処理が正常に進むことはあり得ないので、落とすことを想定してthrow+ログ出しする
                Logger.Fatal(CLASS_NAME, "DragOver", "_model is null!");
                throw new Exception();
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
                Logger.Fatal(CLASS_NAME, "Drop", "_model is null!");
                return;
            }

            if (_model.CanDrop(dropInfo) == false)
            {
                Logger.Fatal(CLASS_NAME, "Drop", "CanDrop return false");
                return;
            }

            // 将来的にはタブを分離させる必要があるので、そのための仮処置
            List<string> displayTargetFilePaths = _model.GetDisplayTextFilePath(dropInfo);
            foreach (string filePath in displayTargetFilePaths)
            {
                _openingFiles.Add(_tabId, filePath);
                ++_tabId;
            }

            // 現状、表示可能テキストは1つだけなので、0番目を使用する
            // 対象の全ファイルを開き、内部で保持する(現状、1つのファイルしか開けないが引数は複数に対応させるだけさせておく)
            _model.SetTextDocuments(_openingFiles);

        }

        #endregion

        #region Execute method

        /// <summary>設定ボタン押下時処理</summary>
        /// <param name="parameter"></param>
        private void ExecuteOpenSettingsWindow(object parameter)
        {
            Logger.Info(CLASS_NAME, "ExecuteOpenSettingsWindow", "start");

            if (_model == null)
            {
                Logger.Fatal(CLASS_NAME, "ExecuteOpenSettingsWindow", "_model is null");
                return;
            }

            _model.OpenSettingsWindow();
        }

        /// <summary>ツールバーの新規作成ボタン押下時処理</summary>
        /// <param name="parameter"></param>
        private void ExecuteCreateNewFile(object parameter)
        {
            Logger.Info(CLASS_NAME, "ExecuteCreateNewFile", "start");

            if (_model == null)
            {
                Logger.Fatal(CLASS_NAME, "ExecuteCreateNewFile", "_model is null");
                return;
            }

            if (_model.CreateNewFile() == false)
            {
                Logger.Error(CLASS_NAME, "ExecuteCreateNewFile", "Create new file failed!");
                return;
            }

            Logger.Info(CLASS_NAME, "ExecuteCreateNewFile", "end");
        }

        /// <summary>ツールバーの「開く」ボタン押下時処理</summary>
        /// <param name="parameter"></param>
        private void ExecuteOpenFile(object parameter)
        {
            Logger.Info(CLASS_NAME, "ExecuteOpenFile", "start");

            if (_model == null)
            {
                Logger.Fatal(CLASS_NAME, "ExecuteOpenFile", "_model is null");
                return;
            }

            _model.OpenFile();
        }

        /// <summary>ツールバーの「名前をつけて保存」押下時処理</summary>
        /// <param name="parameter"></param>
        private void ExecuteSaveAs(object parameter)
        {
            Logger.Info(CLASS_NAME, "ExecuteSaveAs", "start");

            if (_model == null)
            {
                Logger.Fatal(CLASS_NAME, "ExecuteSaveAs", "_model is null");
                return;
            }

            _model.SaveAs();
        }

        /// <summary>Ctrl+S, ツールバーの上書き保存押下時処理</summary>
        /// <param name="parameter"></param>
        private void ExecuteSave(object parameter)
        {
            Logger.Info(CLASS_NAME, "ExecuteSave", "start");

            if (_model == null)
            {
                Logger.Fatal(CLASS_NAME, "ExecuteSave", "_model is null");
                return;
            }

            _model.Save(DisplayTextDoc.Text);
        }

        /// <summary>類語ウィンドウを開く</summary>
        /// <param name="parameter"></param>
        private void ExecuteOpenSynonymWindow(object parameter)
        {
            Logger.Info(CLASS_NAME, "ExecuteOpenSynonymWindow", "start");

            if (_model == null)
            {
                Logger.Fatal(CLASS_NAME, "ExecuteOpenSynonymWindow", "_model is null");
                return;
            }

            _model.OpenSynonymWindow();
        }

        /// <summary>検索処理</summary>
        /// <param name="parameter"></param>
        private void ExecuteSearch(object parameter)
        {
            Logger.Info(CLASS_NAME, "ExecuteSearch", $"start. SearchWord:[{SearchWord}]");

            if (_model == null)
            {
                Logger.Fatal(CLASS_NAME, "ExecuteSearch", "_model is null");
                return;
            }

            if (string.IsNullOrEmpty(SearchWord))
            {
                Logger.Error(CLASS_NAME, "ExecuteSearch", "SearchWord is null or empty!");
                return;
            }

            // dicのintはindex部分なので本文キャレット移動、stringは結果表示リストに使用する
            Dictionary<int, string> indexWordPairs = _model.SearchAllWordsInText(SearchWord, DisplayTextDoc.Text, SEARCHRESULT_MARGIN);
            if (UpdateSearchResultVisiblity(indexWordPairs) == false)
            {
                Logger.Error(CLASS_NAME, "ExecuteSearch", "UpdateSearchResultVisibility return false!");
                return;
            }

            // 旧検索結果をクリアする
            SearchResult.Clear();

            // 念のため昇順にソートしておく
            indexWordPairs.OrderBy(pair => pair.Key);

            SearchResultEntity[] searchResults = new SearchResultEntity[indexWordPairs.Count];
            foreach (KeyValuePair<int, string> kvp in indexWordPairs)
            {
                SearchResult.Add(
                    new SearchResultEntity()
                    {
                        Index = kvp.Key,
                        DisplayWord = kvp.Value
                    }
                    );
            }

            // 検索結果にハイライトをかける
            _model.ApplyHighlightToTarget(SearchWord);
        }

        /// <summary>検索結果へのジャンプ処理</summary>
        /// <param name="parameter"></param>
        private void ExecuteJumpToSearchResult(object parameter)
        {
            Logger.Info(CLASS_NAME, "ExecuteJumpToSearchResult", "start");

            SearchResultEntity searchResultEntity = parameter as SearchResultEntity;
            if (searchResultEntity == null)
            {
                Logger.Fatal(CLASS_NAME, "ExecuteJumpToSearchResult", "searchResultEntity is null!");
                return;
            }

            // キャレットの更新とFocusを行う            
            if (_model == null)
            {
                Logger.Fatal(CLASS_NAME, "ExecuteJumpToSearchResult", "_model is null");
                return;
            }

            if (_model.UpdateCaretOffset(searchResultEntity.Index) == false)
            {
                Logger.Error(CLASS_NAME, "ExecuteJumpToSearchResult", "UpdateCaretOffset return false!");
                return;
            }
        }

        /// <summary>類語検索結果へのジャンプ処理</summary>
        /// <param name="parameter"></param>
        private void ExecuteJumpToSynonymSearchResult(object parameter)
        {
            #region convert args and null check

            if (parameter == null)
            {
                Logger.Fatal(CLASS_NAME, "ExecuteJumpToSynonymSearchResult", "parameter is null");
                return;
            }

            SelectionChangedEventArgs args = parameter as SelectionChangedEventArgs;
            if (args == null || args.AddedItems == null || args.AddedItems.Count < 1)
            {
                Logger.Fatal(CLASS_NAME, "ExecuteJumpToSynonymSearchResult", "args is incollect");
                return;
            }

            DisplaySynonymSearchResult synonym = args.AddedItems[0] as DisplaySynonymSearchResult;
            if (synonym == null)
            {
                Logger.Fatal(CLASS_NAME, "ExecuteJumpToSynonymSearchResult", "synonym is null");
                return;
            }

            if (_model == null)
            {
                Logger.Fatal(CLASS_NAME, "ExecuteJumpToSynonymSearchResult", "_model is null");
                return;
            }

            #endregion

            // キャレットの更新とFocusを行う            
            if (_model.UpdateCaretOffset(synonym.Index) == false)
            {
                Logger.Error(CLASS_NAME, "ExecuteJumpToSynonymSearchResult", "UpdateCaretOffset return false!");
                return;
            }
        }

        /// <summary>画面上部のテキスト情報更新処理</summary>
        /// <param name="parameter"></param>
        private void ExecuteUpdateTextInfo(object parameter)
        {
            // 文書更新時に都度呼び出されるので、異常系以外でログは出さない
            // Logger.InfoLog(CLASS_NAME, "ExecuteUpdateTextInfo", "start");

            if (_model == null || _model.DisplayTextDocument==null)
            {
                WordCount = null;
                NumberOfLines = null;
            }
            else
            {
                WordCount = _model.DisplayTextDocument.TextLength.ToString();
                NumberOfLines = _model.DisplayTextDocument.LineCount.ToString();
            }
        }

        /// <summary>類語グループ選択時に類語一覧と選択中の類語グループIDを更新する</summary>
        /// <param name="parameter"></param>
        private void ExecuteSelectSynonymGroup(object parameter)
        {
            #region convert args and null check

            if (parameter == null)
            {
                Logger.Fatal(CLASS_NAME, "ExecuteSelectSynonymGroup", "parameter is null!");
                return;
            }

            SelectionChangedEventArgs e = parameter as SelectionChangedEventArgs;
            if (e == null)
            {
                Logger.Fatal(CLASS_NAME, "ExecuteSelectSynonymGroup", "EventArgs is null!");
                return;
            }

            object[] obj = e.AddedItems as object[];
            if (obj == null || obj.Any() == false)
            {
                Logger.Fatal(CLASS_NAME, "ExecuteSelectSynonymGroup", "target object is null!");
                return;
            }

            SynonymGroupEntity selectedSynonymGroup = obj[0] as SynonymGroupEntity;
            if (selectedSynonymGroup == null)
            {
                Logger.Fatal(CLASS_NAME, "ExecuteSelectSynonymGroup", "selectedSynonymGroup is null!");
                return;
            }

            #endregion

            if (IsExistSynonymGroupID(selectedSynonymGroup.GroupID) == false)
            {
                Logger.Fatal(CLASS_NAME, "ExecuteSelectSynonymGroup", $"target synonymGroup is not exist! groupID:[{selectedSynonymGroup.GroupID}]");
                return;
            }

            _selectedSynonymGroupId = selectedSynonymGroup.GroupID;
            UpdateDisplaySynonymWords(_selectedSynonymGroupId);
        }

        /// <summary>類語検索処理</summary>
        /// <param name="parameter"></param>
        private void ExecuteSynonymSearch(object parameter)
        {
            Logger.Info(CLASS_NAME, "ExecuteSynonymSearch", "start");

            if (_model == null)
            {
                Logger.Fatal(CLASS_NAME, "ExecuteSynonymSearch", "_model is null!");
                return;
            }

            // 表示するモノがないか、空テキストなら検索する意味がない
            if (_model.DisplayTextDocument == null ||
                string.IsNullOrEmpty(_model.DisplayTextDocument.Text))
            {
                Logger.Error(CLASS_NAME, "ExecuteSynonymSearch", "target text is null or empty!");
                return;
            }

            // 検索するべき類語のすべてを配列に変換
            DisplaySynonymWord[] synonymWords = new DisplaySynonymWord[DisplaySynonymWords.Count];
            DisplaySynonymWords.CopyTo(synonymWords, 0);

            // 類語検索はmodelに依頼
            DisplaySynonymSearchResult[] synonymSearchResults = _model.SynonymSearch(synonymWords, DisplayTextDoc.Text);

            // 現状、特に表示しているモノとの整合性は考えずに更新する
            // メモリの負荷が大きくなってきたら、別途検討することにする
            DisplaySynonymSearchResults.Clear();
            foreach (DisplaySynonymSearchResult result in synonymSearchResults)
            {
                DisplaySynonymSearchResults.Add(result);
            }

            // 検索結果にハイライトをかける
            string[] searchWords = new string[DisplaySynonymWords.Count];
            for (int i = 0; i < DisplaySynonymWords.Count; ++i)
            {
                searchWords[i] = DisplaySynonymWords[i].SynonymWord;
            }
            _model.ApplyHighlightToTargets(searchWords);
        }

        #endregion

        /// <summary>検索結果表示領域のVisibilityを更新します</summary>
        /// <returns>true:検索結果あり、false;検索結果なし</returns>
        private bool UpdateSearchResultVisiblity(Dictionary<int, string> searchResult)
        {
            if (searchResult == null)
            {
                // nullなら表示を隠す
                SearchResultVisibility = Visibility.Hidden;
                return false;
            }
            else if (searchResult.Count < 1)
            {
                // 検索結果がなければ、その旨を表示する
                NoSearchResultVisibility = Visibility.Visible;
                SearchResultVisibility = Visibility.Hidden;
                return false;
            }
            else
            {
                // 検索結果ありの場合、結果を表示できるようにする
                NoSearchResultVisibility = Visibility.Hidden;
                SearchResultVisibility = Visibility.Visible;
            }

            return true;
        }

        /// <summary>編集済み判定処理</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnIsModifiedChanged(object sender, EventArgs e)
        {
            TextEditor textEditor = sender as TextEditor;
            if (textEditor == null)
            {
                Logger.Fatal(CLASS_NAME, "OnIsModifiedChanged", "textEditor is null!");
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

        #region 類語検索結果関連Entity

        /// <summary>表示用の類語一覧リスト</summary>
        public class DisplaySynonymWord
        {
            // todo:色表示用プロパティ        
            /// <summary>類語ID</summary>
            public int WordID { get; set; }

            /// <summary>類語</summary>
            public string SynonymWord { get; set; }

            /// <summary>類語の合計使用回数</summary>
            public int WordCount { get; set; }

            /// <summary>連続使用の合計回数</summary>
            public int RepeatCount { get; set; }
        }

        /// <summary>表示用の類語検索結果</summary>
        public class DisplaySynonymSearchResult
        {
            // todo:色表示用プロパティ

            /// <summary>使用箇所</summary>
            /// <remarks>通常検索の検索結果と同じ考え方</remarks>
            public string UsingSection { get; set; }

            /// <summary>検索元の類語</summary>
            public string SynonymWord { get; set; }

            /// <summary>今の時点で何回目の使用か</summary>
            /// <remarks>最小値は1</remarks>
            public int UsingCount { get; set; }

            /// <summary>連続して使用されている場合、何回続いているか</summary>
            public int RepeatCount { get; set; }

            /// <summary>キャレット遷移用のインデックス</summary>
            public int Index { get; set; }
        }

        #endregion

    }
}
