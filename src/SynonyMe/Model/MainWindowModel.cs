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
using ICSharpCode.AvalonEdit.Document;
using System.Windows.Media;
using SynonyMe.Model.MainWindow;
using System.Windows;
using SynonyMe.CommonLibrary.Entity;

namespace SynonyMe.Model
{
    /// <summary>メイン画面Model</summary>
    /// <remarks>将来タブで複数文書を管理するようになる場合、対象文書:MainWindowVM:本クラスが1:1:1で結びつくようにすること</remarks>
    internal class MainWindowModel
    {
        #region field

        private const string CLASS_NAME = "MainWindowModel";

        /// <summary>ViewModel</summary>
        internal MainWindowVM ViewModel { get; private set; }

        /// <summary>上書き保存を強制的に名前をつけて保存にするフラグ</summary>
        internal bool ForceSaveAs { get; set; } = true;

        //todo:複数タブに対応する場合、検索やツールバーまわりのModelクラスはシングルトン化して不要なインスタンスの生成を防ぐ
        /// <summary>AvalonEdit領域を司るModel</summary>
        private AvalonEditModel _avalonEditModel = null;

        /// <summary>画面左側の検索ペインを管理するModel</summary>
        private SearchModel _searchModel = null;

        /// <summary>画面右側の類語ペインを管理するModel</summary>
        private SynonymModel _synonymModel = null;

        /// <summary>画面上部のツールバーを管理するModel</summary>
        private ToolbarModel _toolbarModel = null;

        /// <summary>本プロセスで処理対象となるファイル拡張子一覧</summary>
        /// <remarks>ここに含まれない拡張子のファイルは読み込み時に弾かれる</remarks>
        internal readonly string[] PROCESS_TARGET_FILE_EXTENSIONS = new string[]
        {
            ".txt"
        };


        internal AvalonEdit.Highlight.HighlightManager HighlightManager { get; private set; }

        private Manager.SettingManager _settingManager = null;

        #endregion

        #region property

        internal bool ShowLineCount { get; private set; }

        internal bool ShowWordCount { get; private set; }

        internal bool ShowNumberOfLines { get; private set; }

        internal bool WordWrap { get; private set; }

        internal int SearchResultMargin { get; private set; }

        internal int SearchResultCount { get; private set; }

        internal string FontFamily { get; private set; }

        internal double FontSize { get; private set; }

        /// <summary>表示中のテキスト文書 </summary>
        /// <remarks>基本的にnullはありえない想定なので、nullだったら都度ログ出しして良いと思う</remarks>
        internal TextDocument TextDocument { get; set; } = new TextDocument();

        /// <summary>テキスト表示オプション設定</summary>
        /// <remarks>内部のプロパティを変更すると、AvalonEdit側で動的に変更してくれる</remarks>
        internal TextEditorOptions TextEditorOptions = new TextEditorOptions()
        {
            EnableImeSupport = true
        };

        /// <summary>画面表示中テキストの絶対パス</summary>
        internal string DisplayTextFilePath;

        #endregion event

        internal event EventHandler UpdateSynonymEvent
        {
            add
            {
                SynonymManager.UpdateSynonymEvent += value;
            }
            remove
            {
                SynonymManager.UpdateSynonymEvent -= value;
            }
        }

        /// <summary>高度な設定変更時に発火するイベントハンドラ</summary>
        internal event EventHandler<Manager.Events.SettingChangedEventArgs> AdvancedSettingChangedEvent
        {
            add
            {
                SettingManager.GetSettingManager.AdvancedSettingChangedEvent += value;
            }
            remove
            {
                SettingManager.GetSettingManager.AdvancedSettingChangedEvent -= value;
            }
        }

        /// <summary>一般設定変更時に発火するイベントハンドラ</summary>
        internal event EventHandler<Manager.Events.SettingChangedEventArgs> GeneralSettingChangedEvent
        {
            add
            {
                SettingManager.GetSettingManager.GeneralSettingChangedEvent += value;
            }
            remove
            {
                SettingManager.GetSettingManager.GeneralSettingChangedEvent -= value;
            }
        }

        /// <summary>検索・類語検索設定変更時に発火するイベントハンドラ</summary>
        internal event EventHandler<Manager.Events.SettingChangedEventArgs> SearchAndSynonymSettingChangedEvent
        {
            add
            {
                SettingManager.GetSettingManager.SearchAndSynonymSettingChangedEvent += value;
            }
            remove
            {
                SettingManager.GetSettingManager.SearchAndSynonymSettingChangedEvent -= value;
            }
        }


        #region method

        /// <summary>コンストラクタ</summary>
        /// <param name="viewModel">メンバに保持するVM</param>
        internal MainWindowModel(MainWindowVM viewModel)
        {
            ViewModel = viewModel;
            Initialize();
            InitializeInternalModel();
        }

        private void Initialize()
        {
            // ★必ず最初に取得すること->singleton生成に伴う設定読込のため
            _settingManager = SettingManager.GetSettingManager;

            HighlightManager = new AvalonEdit.Highlight.HighlightManager(ViewModel.AvalonEditBackGround);//★
        }

        /// <summary>MainWindowModelの機能をさらに細分化して管理しているModelクラスを初期化します</summary>
        private void InitializeInternalModel()
        {
            _avalonEditModel = new AvalonEditModel(this);
            _searchModel = new SearchModel(this);
            _synonymModel = new SynonymModel(this);
            _toolbarModel = new ToolbarModel(this);
        }

        /// <summary>SynonyMeで管理している全設定情報を適用します</summary>
        internal void ApplySettings()
        {
            ApplyGeneralSetting();
            ApplySearchAndSynonymSetting();
            ApplyAdvancedSetting();
        }

        /// <summary>一般設定情報を適用します</summary>
        private void ApplyGeneralSetting()
        {
            if (_settingManager == null)
            {
                Logger.Fatal(CLASS_NAME, "ApplyGeneralSetting", "_settingManager is null!");
                return;
            }

            UpdateGeneralSetting(_settingManager.GetSetting(typeof(Settings.GeneralSetting)) as Settings.GeneralSetting);
        }

        /// <summary>検索・類語検索設定を適用します</summary>
        private void ApplySearchAndSynonymSetting()
        {
            if (_settingManager == null)
            {
                Logger.Fatal(CLASS_NAME, "ApplySearchAndSynonymSetting", "_settingManager is null!");
                return;
            }

            UpdateSearchAndSynonymSetting(_settingManager.GetSetting(typeof(Settings.SearchAndSynonymSetting)) as Settings.SearchAndSynonymSetting);
        }

        /// <summary>高度な設定を適用します</summary>
        private void ApplyAdvancedSetting()
        {
            if (_settingManager == null)
            {
                Logger.Fatal(CLASS_NAME, "ApplyAdvancedSetting", "_settingManager is null!");
                return;
            }

            UpdateAdvancedSetting(_settingManager.GetSetting(typeof(Settings.AdvancedSetting)) as Settings.AdvancedSetting);
        }

        /// <summary>一般設定を要求された設定情報で更新します</summary>
        /// <param name="setting"></param>
        internal void UpdateGeneralSetting(Settings.GeneralSetting setting)
        {
            if (setting == null)
            {
                Logger.Error(CLASS_NAME, "UpdateGeneralSetting", "setting is null!");
                return;
            }

            //todo:VMのプロパティに直に代入すれば良いのでは？
            ShowLineCount = setting.ShowingLineCount;
            ViewModel.NotifyPropertyChanged("LineCountVisible");

            ShowNumberOfLines = setting.ShowingNumberOfLines;
            ViewModel.NotifyPropertyChanged("CanShowNumberOfLines");

            ShowWordCount = setting.ShowingWordCount;
            ViewModel.NotifyPropertyChanged("WordCountVisible");

            TextEditorOptions.ShowEndOfLine = setting.ShowingNewLine;
            TextEditorOptions.ShowSpaces = setting.ShowingSpace;
            TextEditorOptions.ShowTabs = setting.ShowingTab;
            ViewModel.NotifyPropertyChanged("TextEditorOptions");

            WordWrap = setting.WrappingText;
            ViewModel.NotifyPropertyChanged("WordWrap");

            FontFamily = setting.MainFontName + ", " + setting.SubFontName;
            ViewModel.NotifyPropertyChanged("FontFamily");

            FontSize = setting.FontSize;
            ViewModel.NotifyPropertyChanged("FontSize");

            ViewModel.AvalonEditBackGround = new SolidColorBrush(CommonLibrary.ConversionUtility.ConversionColorCodeToColor(setting.WallPaperColor));
            ViewModel.NotifyPropertyChanged("AvalonEditBackGround");

            ViewModel.AvalonEditForeGround = new SolidColorBrush(CommonLibrary.ConversionUtility.ConversionColorCodeToColor(setting.FontColor));
            ViewModel.NotifyPropertyChanged("AvalonEditForeGround");
        }

        internal void UpdateSearchAndSynonymSetting(Settings.SearchAndSynonymSetting setting)
        {
            if (setting == null)
            {
                Logger.Error(CLASS_NAME, "UpdateSearchAndSynonymSetting", "setting is null!");
                return;
            }

            //todo:固定値割り振り
            SearchResultMargin = setting.SearchResultMargin;
            SearchResultCount = setting.SearchResultDisplayCount;
        }

        internal void UpdateAdvancedSetting(Settings.AdvancedSetting setting)
        {
            if (setting == null)
            {
                return;
            }

            //todo:固定値割り振り
            List<string> targetFileList = setting.TargetFileExtensionList;
        }

        /// <summary>ハイライトを対象語句にそれぞれ適用します</summary>
        /// <param name="targets">対象語句</param>
        /// <returns>true:成功, false:失敗</returns>
        internal bool ApplyHighlightToTargets(string[] targets, CommonLibrary.ApplyHighlightKind kind) //todo:検索か類語検索かの判別しないとFontColorとBackGroundが分けられない
        {
            if (targets == null || targets.Any() == false)
            {
                Logger.Fatal(CLASS_NAME, "ApplyHighlightToTargets", "targets is null or empty!");
                return false;
            }

            if (HighlightManager == null)
            {
                Logger.Fatal(CLASS_NAME, "ApplyHighlightToTargets", "HighlightManager is null!");
                return false;
            }

            return HighlightManager.UpdateXshdFile(targets, kind);
        }

        /// <summary>指定された語句にハイライトを適用します</summary>
        /// <param name="target">対象語句</param>
        /// <returns>true:成功, false:失敗</returns>
        internal bool ApplyHighlightToSearchResult(string target)
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

            return ApplyHighlightToTargets(targets, CommonLibrary.ApplyHighlightKind.Search);
        }

        /// <summary>ドラッグオーバー中のファイルがドロップ可能かを調べる</summary>
        /// <returns>true:ドロップ可能、false:ドロップ不可能(何か1つでも不可能な場合)</returns>
        internal void ChangeDragOverMouseEffect(IDropInfo dropInfo)
        {
            if (_avalonEditModel == null)
            {
                Logger.Fatal(CLASS_NAME, "ChangeDragOverMouseEffect", "_avalonEditModel is null!");
                return;
            }

            _avalonEditModel.ChangeDragOverMouseEffect(dropInfo);
        }

        /// <summary>ドロップされたファイルの保持と展開処理を行います</summary>
        /// <param name="dropInfo"></param>
        internal void Drop(IDropInfo dropInfo)
        {
            if (_avalonEditModel == null)
            {
                Logger.Fatal(CLASS_NAME, "Drop", "_avalonEditModel is null!");
                return;
            }

            _avalonEditModel.Drop(dropInfo);
        }

        #region Toolbar

        /// <summary>渡されたファイル情報に基づいて保存処理を実行する</summary>
        /// <param name="filePath">保存対象ファイルパス</param>
        /// <param name="displayText">保存したいテキスト情報</param>
        /// <returns>true:成功, false:失敗</returns>
        internal void Save()
        {
            if (_toolbarModel == null)
            {
                Logger.Fatal(CLASS_NAME, "Save", "_toolbarModel is null!");
                return;
            }

            if (_toolbarModel.Save())
            {
                Logger.Info(CLASS_NAME, "Save", "Save successed!");
            }
            else
            {
                Logger.Error(CLASS_NAME, "Save", "Save failed!");
            }

        }

        /// <summary>名前をつけて保存</summary>
        /// <returns>true:成功, false:失敗</returns>
        internal void SaveAs()
        {
            if (_toolbarModel == null)
            {
                Logger.Fatal(CLASS_NAME, "SaveAs", "_toolbarModel is null!");
                return;
            }

            if (_toolbarModel.SaveAs())
            {
                Logger.Info(CLASS_NAME, "SaveAs", "SaveAs successed!");
            }
            else
            {
                Logger.Error(CLASS_NAME, "SaveAs", "SaveAs failed!");
            }
        }

        /// <summary>類語ウィンドウを開く</summary>
        internal void OpenSynonymWindow()
        {
            WindowManager.OpenSubWindow(CommonLibrary.SubWindowName.SynonymWindow);
        }

        /// <summary>設定ウィンドウを開く</summary>
        internal void OpenSettingsWindow()
        {
            WindowManager.OpenSubWindow(CommonLibrary.SubWindowName.SettingWindow);
        }

        /// <summary>ファイルを開くダイアログを表示し、既存のファイルを読み込みます</summary>
        internal void OpenFile()
        {
            if (_toolbarModel == null)
            {
                Logger.Fatal(CLASS_NAME, "OpenFile", "_toolbarModel is null!");
                return;
            }

            if (_toolbarModel.OpenFile())
            {
                Logger.Info(CLASS_NAME, "OpenFile", "OpenFile successed!");
            }
            else
            {
                Logger.Error(CLASS_NAME, "OpenFile", "OpenFile failed!");
            }
        }

        /// <summary>テキストファイルを新規作成します</summary>
        internal void CreateNewFile()
        {
            if (_toolbarModel == null)
            {
                Logger.Fatal(CLASS_NAME, "CreateNewFile", "_toolbarModel is null!");
                return;
            }

            if (_toolbarModel.CreateNewFile())
            {
                Logger.Info(CLASS_NAME, "CreateNewFile", $"CreateNewFile successed! Displaying text:[{DisplayTextFilePath}]");
                return;
            }
            else
            {
                Logger.Error(CLASS_NAME, "CreateNewFile", $"CreateNewFile failed. Displaying text:[{DisplayTextFilePath}]");
                return;
            }
        }

        #endregion

        /// <summary>DBに登録されている全類語グループを取得する</summary>
        /// <returns>正常時：DBに登録されている全類語グループ、異常時:false</returns>
        internal CommonLibrary.Entity.SynonymGroupEntity[] GetAllSynonymGroups()
        {
            return SynonymManager.GetAllSynonymGroup();
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
            if (ViewModel == null)
            {
                Logger.Fatal(CLASS_NAME, "SynonymSearch", "ViewModel is null");
                return null;
            }

            return Searcher.GetSearcher.SynonymSearch(targetSynonyms, targetText, SearchResultMargin, SearchResultCount);
        }

        /// <summary>
        /// キャレットの移動を行う
        /// </summary>
        /// <param name="index">カーソル配置位置</param>
        /// <returns>true:正常、false:異常</returns>
        /// <remarks>todo:MainWindowから画面要素取得しないでもなんとかならないか</remarks>
        internal bool UpdateCaretOffset(int index)
        {
            if (index < 0)
            {
                Logger.Error(CLASS_NAME, "UpdateCaretOffset", $"index is incorrect. index:[{index}]");
                return false;
            }

            View.MainWindow mw = WindowManager.GetMainWindow();
            if (mw == null)
            {
                Logger.Fatal(CLASS_NAME, "UpdateCaretOffset", "mw is null!");
                return false;
            }

            TextEditor te = mw.TextEditor;
            if (te == null)
            {
                Logger.Fatal(CLASS_NAME, "UpdateCaretOffset", "te is null!");
                return false;
            }

            // キャレット更新のためにMainWindowと紐付いているTextEditorを取得する必要がある
            te.CaretOffset = index;
            te.TextArea.Caret.BringCaretToView();

            // BeginInvokeしないとFocusしてくれない
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() => { te.Focus(); }));

            return true;
        }

        /// <summary>検索処理を行います</summary>
        /// <param name="searchWord"></param>
        /// <param name="text"></param>
        internal void Search(string searchWord, string text)
        {
            _searchModel.Search(searchWord, text);
        }


        #endregion
    }
}
