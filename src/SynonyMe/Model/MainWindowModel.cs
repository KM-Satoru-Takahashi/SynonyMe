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

namespace SynonyMe.Model
{
    /// <summary>メイン画面Model</summary>
    /// <remarks>将来タブで複数文書を管理するようになる場合、対象文書:MainWindowVM:本クラスが1:1:1で結びつくようにすること</remarks>
    internal class MainWindowModel
    {
        #region field

        private const string CLASS_NAME = "MainWindowModel";

        /// <summary>ViewModel</summary>
        private MainWindowVM _viewModel = null;

        /// <summary>上書き保存を強制的に名前をつけて保存にするフラグ</summary>
        private bool _forceSaveAs = true;

        /// <summary>本プロセスで処理対象となるファイル拡張子一覧</summary>
        /// <remarks>ここに含まれない拡張子のファイルは読み込み時に弾かれる</remarks>
        private static readonly string[] PROCESS_TARGET_FILE_EXTENSIONS = new string[]
        {
            ".txt"
        };

        private AvalonEdit.Highlight.HighlightManager _highlightManager = null;

        private Manager.SettingManager _settingManager = null;

        #endregion

        #region property

        /// <summary>true:表示中のテキストが編集済み, false:未編集または保存済み</summary>
        internal bool IsModified { get; set; } = false;

        internal bool ShowLineCount { get; private set; }

        internal bool ShowWordCount { get; private set; }

        internal bool ShowNumberOfLines { get; private set; }

        internal bool WordWrap { get; private set; }

        internal int SearchResultMargin { get; private set; }

        internal int SearchResultCount { get; private set; }

        internal string FontFamily { get; private set; }

        internal double FontSize { get; private set; }

        /// <summary>文章1つにつき1つ割り当てられるAvalonEditインスタンス※Textはここからではなく、DisplayTextDocumentから取ること※</summary>
        /// <remarks>複数文章を表示する改修を行う場合、Dictionaryで文章とTextEditorを紐付けて管理する必要あり</remarks>
        /// todo:改行や編集記号等の表示もMainWindow側のTextEditorで行える、以下のプロパティ
        /// Options.ShowEndOfLine, ShowSpaces, ShowTabs, ShowBoxForControlCharacters

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
            _viewModel = viewModel;
            Initialize();
        }

        private void Initialize()
        {
            // ★必ず最初に取得すること->singleton生成に伴う設定読込のため
            _settingManager = SettingManager.GetSettingManager;

            _highlightManager = new AvalonEdit.Highlight.HighlightManager(_viewModel.AvalonEditBackGround);//★
        }


        internal void ApplySettings()
        {
            ApplyGeneralSetting();
            ApplySearchAndSynonymSetting();
            ApplyAdvancedSetting();
        }


        private void ApplyGeneralSetting()
        {
            if (_settingManager == null)
            {
                Logger.Fatal(CLASS_NAME, "ApplyGeneralSetting", "_settingManager is null!");
                return;
            }

            UpdateGeneralSetting(_settingManager.GetSetting(typeof(Settings.GeneralSetting)) as Settings.GeneralSetting);
        }

        private void ApplySearchAndSynonymSetting()
        {
            if (_settingManager == null)
            {
                Logger.Fatal(CLASS_NAME, "ApplySearchAndSynonymSetting", "_settingManager is null!");
                return;
            }

            UpdateSearchAndSynonymSetting(_settingManager.GetSetting(typeof(Settings.SearchAndSynonymSetting)) as Settings.SearchAndSynonymSetting);
        }

        private void ApplyAdvancedSetting()
        {
            if (_settingManager == null)
            {
                Logger.Fatal(CLASS_NAME, "ApplyAdvancedSetting", "_settingManager is null!");
                return;
            }

            UpdateAdvancedSetting(_settingManager.GetSetting(typeof(Settings.AdvancedSetting)) as Settings.AdvancedSetting);
        }

        internal void UpdateGeneralSetting(Settings.GeneralSetting setting)
        {
            if (setting == null)
            {
                Logger.Error(CLASS_NAME, "UpdateGeneralSetting", "setting is null!");
                return;
            }


            //todo:VMのプロパティに直に代入すれば良いのでは？
            ShowLineCount = setting.ShowingLineCount;
            _viewModel.NotifyPropertyChanged("LineCountVisible");

            ShowNumberOfLines = setting.ShowingNumberOfLines;
            _viewModel.NotifyPropertyChanged("CanShowNumberOfLines");

            ShowWordCount = setting.ShowingWordCount;
            _viewModel.NotifyPropertyChanged("WordCountVisible");

            TextEditorOptions.ShowEndOfLine = setting.ShowingNewLine;
            TextEditorOptions.ShowSpaces = setting.ShowingSpace;
            TextEditorOptions.ShowTabs = setting.ShowingTab;
            _viewModel.NotifyPropertyChanged("TextEditorOptions");

            WordWrap = setting.WrappingText;
            _viewModel.NotifyPropertyChanged("WordWrap");

            FontFamily = setting.MainFontName + ", " + setting.SubFontName;
            _viewModel.NotifyPropertyChanged("FontFamily");

            FontSize = setting.FontSize;
            _viewModel.NotifyPropertyChanged("FontSize");

            _viewModel.AvalonEditBackGround = new SolidColorBrush(CommonLibrary.ConversionUtility.ConversionColorCodeToColor(setting.WallPaperColor));
            _viewModel.NotifyPropertyChanged("AvalonEditBackGround");

            _viewModel.AvalonEditForeGround = new SolidColorBrush(CommonLibrary.ConversionUtility.ConversionColorCodeToColor(setting.FontColor));
            _viewModel.NotifyPropertyChanged("AvalonEditForeGround");
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

            if (_highlightManager == null)
            {
                Logger.Fatal(CLASS_NAME, "ApplyHighlightToTargets", "_highlightManager is null!");
                return false;
            }

            return _highlightManager.UpdateXshdFile(targets, kind);
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
            if (_forceSaveAs)
            {
                return SaveAs(displayText);
            }

            if (string.IsNullOrEmpty(DisplayTextFilePath) ||
               displayText == null) // displayTextは空文字の場合emptyはあり得る
            {
                Logger.Fatal(CLASS_NAME, "Save", "filePath or displayText is null or empty!");
                return false;
            }

            if (TextDocument == null)
            {
                Logger.Fatal(CLASS_NAME, "Save", "DisplayTextDocument is null!");
                return false;
            }

            try
            {
                TextDocument.Text = displayText;
                TextEditor textEditor = new TextEditor
                {
                    Document = TextDocument
                };
                textEditor.Save(DisplayTextFilePath);
            }
            catch (Exception e)
            {
                Logger.Fatal(CLASS_NAME, "Save", e.ToString());
                return false;
            }

            // AvalonEditの編集済みフラグをOffにする
            IsModified = false;

            return true;
        }

        /// <summary>名前をつけて保存</summary>
        /// <returns>true:成功, false:失敗</returns>
        internal bool SaveAs()
        {
            return SaveAs(TextDocument.Text);
        }

        /// <summary>名前をつけて保存</summary>
        /// <param name="displayTest">保存対象テキスト</param>
        /// <returns></returns>
        private bool SaveAs(string displayTest)
        {
            Logger.Info(CLASS_NAME, "SaveAs", "start");

            string saveFilePath = string.Empty;
            bool result = DialogManager.GetDialogManager.OpenSaveAsDialog(out saveFilePath);

            // 失敗時はログとエラーダイアログを出す
            if (result == false)
            {
                Logger.Error(CLASS_NAME, "SaveAs", "SaveAs Failed!");

                // todo error dialog
                return false;
            }

            if (FileAccessor.GetFileAccessor.SaveFile(TextDocument.Text, saveFilePath) == false)
            {
                Logger.Error(CLASS_NAME, "SaveAs", $"SaveFile Failed. saveFilePath:[{saveFilePath}]");
                return false;
            }

            // 保持している、現在開いているファイル情報を更新する
            DisplayTextFilePath = saveFilePath;

            // AvalonEditの編集済みフラグをOffにする
            IsModified = false;

            // 名前をつけて保存フラグをOffにする
            _forceSaveAs = false;
            return true;
        }

        /// <summary>類語ウィンドウを開く</summary>
        internal void OpenSynonymWindow()
        {
            WindowManager.OpenSubWindow(CommonLibrary.SubWindowName.SynonymWindow);
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
                //todo:FileStreamで素直に読み込む？
                //textEditorが残って悪さしていないことを確認すること
                TextEditor textEditor = new TextEditor();
                textEditor.Load(filePath);
                TextDocument.Text = textEditor.Text;
            }
            catch (Exception e)
            {
                Logger.Fatal(CLASS_NAME, "Load", e.ToString());
                return false;
            }

            text = TextDocument.Text;
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
        /// <returns>文章内の検索対象index, margin含めた検索結果のdictionary</returns>
        internal Dictionary<int, string> SearchAllWordsInText(string searchWord, string targetText)
        {
            return Searcher.GetSearcher.SearchAllWordsInText(searchWord, targetText, SearchResultMargin, SearchResultCount);
        }

        /// <summary>設定ウィンドウを開く</summary>
        internal void OpenSettingsWindow()
        {
            WindowManager.OpenSubWindow(CommonLibrary.SubWindowName.SettingWindow);
        }

        /// <summary>ファイルを開くダイアログを表示し、既存のファイルを読み込みます</summary>
        internal void OpenFile()
        {
            // 現在表示中のテキストが編集済みか否かを判定する
            if (IsModified)
            {
                // 保存されていなければ、Ok/Cancelダイアログを出して確認する
                DialogResult dialogResult = DialogResult.Cancel;
                bool result = DialogManager.GetDialogManager.OpenOkCancelDialog("現在表示中の文章は保存されていません。\n編集を破棄し、新規にファイルを開いて良いですか？\n(※未保存のテキストは破棄されます！)", out dialogResult);
                if (result == false)
                {
                    Logger.Fatal(CLASS_NAME, "OpenFile", $"Dialog error! dialogResult:[{dialogResult}]");
                    return;
                }

                if (dialogResult == DialogResult.Cancel)
                {
                    Logger.Info(CLASS_NAME, "OpenFile", "Canceled discard text and open new file");
                    return;
                }
            }

            // 破棄OKか、保存済みであれば現在表示中のテキストとXshdをクリアする
            DisposeTextAndXshd();

            // ファイルを開く
            string openFilePath;
            if (DialogManager.GetDialogManager.OpenFileOpenDialog(out openFilePath) == false)
            {
                Logger.Error(CLASS_NAME, "OpenFile", "OpenFileOpenDialog failed.");
                return;
            }

            if (string.IsNullOrEmpty(openFilePath))
            {
                Logger.Fatal(CLASS_NAME, "OpenFile", "Filename is null or empty!");
                return;
            }

            // MainWindowのAvalonEditに適用する
            // 保存したファイルパスを保持する
            DisplayTextFilePath = openFilePath;

            string loadText;
            if (Load(openFilePath, out loadText) == false)
            {
                Logger.Error(CLASS_NAME, "OpenFile", $"Load error. File path:[{openFilePath}]");
                return;
            }

            if (TextDocument == null)
            {
                Logger.Error(CLASS_NAME, "OpenFile", "DisplayTextDocument is null!");
                return;
            }

            TextDocument.Text = loadText;

            // 編集済みフラグを下げる
            IsModified = false;
            _forceSaveAs = false;
        }

        /// <summary>テキストファイルを新規作成します</summary>
        /// <remarks>true:正常, false:異常</remarks>
        internal bool CreateNewFile()
        {
            // 現在表示中のテキストが編集済みか否かを判定する
            if (IsModified)
            {
                // 保存されていなければ、Yes/Noダイアログを出して確認する
                DialogResult dialogResult = DialogResult.Cancel;
                bool result = DialogManager.GetDialogManager.OpenOkCancelDialog("現在編集中の文章を破棄しますか？", out dialogResult);
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
            TextDocument.Text = string.Empty;
            _highlightManager.ResetHighlightInfo();

            // ファイル保存ダイアログを表示する
            string saveFilePath = null;
            if (DialogManager.GetDialogManager.OpenSaveAsDialog(out saveFilePath) == false)
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
            FileAccessor.GetFileAccessor.SaveNewFile(saveFilePath);

            // 保存したファイルパスを保持する
            DisplayTextFilePath = saveFilePath;

            // 編集済みフラグを下げる
            IsModified = false;

            return true;
        }

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
            if (_viewModel == null)
            {
                Logger.Fatal(CLASS_NAME, "SynonymSearch", "_viewModel is null");
                return null;
            }

            return Searcher.GetSearcher.SynonymSearch(targetSynonyms, targetText, SearchResultMargin, SearchResultCount);
        }

        /// <summary>渡された全類語を対象の文章内から検索する</summary>
        /// <param name="targetSynonymWords">検索対象の全類語</param>
        /// <param name="targetText">検索先の文章</param>
        /// <returns>正常時：検索結果、異常時：null</returns>
        private List<MainWindowVM.DisplaySynonymSearchResult> GetAllSynonymSearchResult(MainWindowVM.DisplaySynonymWord[] targetSynonymWords, string targetText)
        {
            if (_viewModel == null)
            {
                Logger.Fatal(CLASS_NAME, "GetAllSynonymSearchResult", "_viewModel is null!");
                return null;
            }

            return Searcher.GetSearcher.GetAllSynonymSearchResult(targetSynonymWords, targetText, SearchResultMargin, SearchResultCount);
        }

        /// <summary>表示中のテキストと、ハイライト表示情報を破棄します</summary>
        /// <remarks>本当にAvalonEditのDisposeがこれだけで十分かは要検討</remarks>
        private void DisposeTextAndXshd()
        {
            if (TextDocument != null)
            {
                TextDocument.Text = string.Empty;
            }

            if (_highlightManager != null)
            {
                _highlightManager.ResetHighlightInfo();
            }
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

        /// <summary>依頼された全ファイルをTextEditorで管理させる</summary>
        /// <param name="openingFiles"></param>
        internal void SetTextDocuments(Dictionary<int, string> openingFiles)
        {
            if (openingFiles == null)
            {
                Logger.Error(CLASS_NAME, "SetTextDocuments", "openingFiles are null or empty!");
                return;
            }

            DisplayTextFilePath = openingFiles[0];
            string text;
            Load(DisplayTextFilePath, out text);
            TextDocument.Text = text;

            // ドロップ直後に「編集済み」が出るのを抑制する
            _viewModel.IsModified = false;//todo:check null

            // Ctrl + Sで名前をつけて保存にしなくて良くする
            _forceSaveAs = false;
        }

        #endregion
    }
}
