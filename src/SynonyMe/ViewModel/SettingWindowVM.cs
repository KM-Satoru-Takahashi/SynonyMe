using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SynonyMe.ViewModel
{
    public class SettingWindowVM : ViewModelBase
    {
        #region field

        private const string CLASS_NAME = "SettingWindowVM";


        #endregion

        #region property

        #region bindings

        #region command

        /// <summary>ウィンドウタイトル</summary>
        public string WindowTitle { get; private set; }

        /// <summary>タブ「文書設定」ヘッダ</summary>
        public string Header_TextSetting { get; private set; }

        /// <summary>タブ「検索・類語設定」ヘッダ</summary>
        public string Header_SearchAndSynonymSetting { get; private set; }

        /// <summary>タブ「高度な設定」ヘッダ</summary>
        public string Header_AdvancedSetting { get; private set; }

        /// <summary>OKボタン押下時コマンド</summary>
        public ICommand Command_Ok { get; private set; }


        public ICommand Command_Cancel { get; private set; }


        public ICommand Command_Apply { get; private set; }


        public ICommand Command_ResetToDefault { get; private set; }


        public ICommand Command_DeleteAllSynonyms { get; private set; }

        #endregion

        #region Tab_AdvancedSettings

        private double _logOutputLevel = 1; //todo:将来的にModel側で管理させること
        public double LogOutputLevel
        {
            get
            {
                return _logOutputLevel;
            }
            set
            {
                if (_logOutputLevel != value)
                {
                    _logOutputLevel = value;
                    UpdateLogLevelVisible(ConvertDoubleToLogLevel(_logOutputLevel));
                    OnPropertyChanged("LogOutputLevel");
                }
            }
        }


        private Visibility _debugVisible = Visibility.Visible;
        public Visibility DebugVisible
        {
            get
            {
                return _debugVisible;
            }
            set
            {
                if (_debugVisible != value)
                {
                    _debugVisible = value;
                    OnPropertyChanged("DebugVisible");
                }
            }
        }

        private Visibility _infoVisible = Visibility.Visible;
        public Visibility InfoVisible
        {
            get
            {
                return _infoVisible;
            }
            set
            {
                if (_infoVisible != value)
                {
                    _infoVisible = value;
                    OnPropertyChanged("InfoVisible");
                }
            }
        }


        private Visibility _warnVisible = Visibility.Visible;
        public Visibility WarnVisible
        {
            get
            {
                return _warnVisible;
            }
            set
            {
                if (_warnVisible != value)
                {
                    _warnVisible = value;
                    OnPropertyChanged("WarnVisible");
                }
            }
        }


        private Visibility _errorVisible = Visibility.Visible;
        public Visibility ErrorVisible
        {
            get
            {
                return _errorVisible;
            }
            set
            {
                if (_errorVisible != value)
                {
                    _errorVisible = value;
                    OnPropertyChanged("ErrorVisible");
                }
            }
        }


        private Visibility _fatalVisible = Visibility.Visible;
        public Visibility FatalVisible
        {
            get
            {
                return _fatalVisible;
            }
            set
            {
                if (_fatalVisible != value)
                {
                    _fatalVisible = value;
                    OnPropertyChanged("FatalVisible");
                }
            }
        }


        private double _logRetentionDays = 1;
        public double LogRetentionDays
        {
            get
            {
                return _logRetentionDays;
            }
            set
            {
                if (_logRetentionDays != value)
                {
                    _logRetentionDays = value;
                    LogRetentionDaysString = value.ToString();
                    OnPropertyChanged("LogRetentionDays");
                }
            }
        }


        private string _logRetentionDaysString = "   ";
        public string LogRetentionDaysString
        {
            get
            {
                return _logRetentionDaysString;
            }
            set
            {
                // 将来のことを考えて3桁に整形する
                if (_logRetentionDaysString != value)
                {
                    _logRetentionDaysString = string.Format("{0,-3}", value);
                    OnPropertyChanged("LogRetentionDaysString");
                }
            }
        }


        private bool _useFastSearch = false;
        public bool UseFastSearch
        {
            get
            {
                return _useFastSearch;
            }
            set
            {
                if (_useFastSearch != value)
                {
                    _useFastSearch = value;
                    OnPropertyChanged("UseFastSearch");
                }
            }
        }


        private string _searchResultDisplayCount = "100";
        public string SearchResultDisplayCount
        {
            get
            {
                return _searchResultDisplayCount;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }
                if (System.Text.RegularExpressions.Regex.IsMatch(value, CommonLibrary.Define.REGEX_NUMBER_ONLY) == false)
                {
                    return;
                }

                if (_searchResultDisplayCount != value)
                {
                    _searchResultDisplayCount = value;
                    OnPropertyChanged("SearchResultDisplayCount");
                }
            }
        }


        private bool _isTxtTarget = false;
        public bool IsTxtTarget
        {
            get
            {
                return _isTxtTarget;
            }
            set
            {
                if (_isTxtTarget != value)
                {
                    _isTxtTarget = value;
                    OnPropertyChanged("IsTxtTarget");
                }
            }
        }


        #endregion

        #region Tab_TextSettings

        private bool _wrappingText = false;
        public bool WrappingText
        {
            get
            {
                return _wrappingText;
            }
            set
            {
                if (_wrappingText != value)
                {
                    _wrappingText = value;
                    OnPropertyChanged("WrappingText");
                }
            }
        }

        private bool _showingLineCount = false;
        public bool ShowingLineCount
        {
            get
            {
                return _showingLineCount;
            }
            set
            {
                if (_showingLineCount != value)
                {
                    _showingLineCount = value;
                    OnPropertyChanged("ShowingLineCount");
                }
            }
        }

        private bool _showingLineNumber = false;
        public bool ShowingLineNumber
        {
            get
            {
                return _showingLineNumber;
            }
            set
            {
                if (_showingLineNumber != value)
                {
                    _showingLineNumber = value;
                    OnPropertyChanged("ShowingLineNumber");
                }
            }
        }

        private bool _showingWordCount = false;
        public bool ShowingWordCount
        {
            get
            {
                return _showingWordCount;
            }
            set
            {
                if (_showingWordCount != value)
                {
                    _showingWordCount = value;
                    OnPropertyChanged("ShowingWordCount");
                }
            }
        }

        private bool _showingNewLine = false;
        public bool ShowingNewLine
        {
            get
            {
                return _showingNewLine;
            }
            set
            {
                if (_showingNewLine != value)
                {
                    _showingNewLine = value;
                    OnPropertyChanged("ShowingNewLine");
                }
            }
        }

        private bool _showingTab = false;
        public bool ShowingTab
        {
            get
            {
                return _showingTab;
            }
            set
            {
                if (_showingTab != value)
                {
                    _showingTab = value;
                    OnPropertyChanged("ShowingTab");
                }
            }
        }

        private bool _showingSpace = false;
        public bool ShowingSpace
        {
            get
            {
                return _showingSpace;
            }
            set
            {
                if (_showingSpace != value)
                {
                    _showingSpace = value;
                    OnPropertyChanged("ShowingSpace");
                }
            }
        }

        private string _fontSize = "0.0";
        public string FontSize
        {
            get
            {
                return _fontSize;
            }
            set
            {
                if (_fontSize != value)
                {
                    _fontSize = value;
                    OnPropertyChanged("FontSize");
                }
            }
        }

        //todo:このあたりすべて：設定ファイルからの読み込み
        private Color _fontColor = new Color();
        public Color FontColor
        {
            get
            {
                return _fontColor;
            }
            set
            {
                if (_fontColor != value)
                {
                    _fontColor = value;
                    OnPropertyChanged("FontColor");
                }
            }
        }

        private string _japaneseFontName = "Meiryo";
        public string JapaneseFontName
        {
            get
            {
                return _japaneseFontName;
            }
            set
            {
                if (_japaneseFontName != value)
                {
                    _japaneseFontName = value;
                    OnPropertyChanged("JapaneseFontName");
                }
            }
        }

        private string _englishFontName = "Consolas";
        public string EnglishFontName
        {
            get
            {
                return _englishFontName;
            }
            set
            {
                if (_englishFontName != value)
                {
                    _englishFontName = value;
                    OnPropertyChanged("EnglishFontName");
                }
            }
        }

        #endregion

        #region Tab_SearchAndSynonymSettings

        private Color _searchResultBackGround = new Color();
        public Color SearchResultBackGround
        {
            get
            {
                return _searchResultBackGround;
            }
            set
            {
                _searchResultBackGround = value;
                OnPropertyChanged("SearchResultBackGround");
            }
        }

        private Color _searchResultFontColor = new Color();
        public Color SearchResultFontColor
        {
            get
            {
                return _searchResultFontColor;
            }
            set
            {
                _searchResultFontColor = value;
                OnPropertyChanged("SearchResultFontColor");
            }
        }

        private Color _synonymSearchResultColor1 = new Color();
        public Color SynonymSearchResultColor1
        {
            get
            {
                return _synonymSearchResultColor1;
            }
            set
            {
                _synonymSearchResultColor1 = value;
                OnPropertyChanged("SynonymSearchResultColor1");
            }
        }

        private Color _synonymSearchResultColor2 = new Color();
        public Color SynonymSearchResultColor2
        {
            get
            {
                return _synonymSearchResultColor2;
            }
            set
            {
                _synonymSearchResultColor2 = value;
                OnPropertyChanged("SynonymSearchResultColor2");
            }
        }

        private Color _synonymSearchResultColor3 = new Color();
        public Color SynonymSearchResultColor3
        {
            get
            {
                return _synonymSearchResultColor3;
            }
            set
            {
                _synonymSearchResultColor3 = value;
                OnPropertyChanged("SynonymSearchResultColor3");
            }
        }


        private Color _synonymSearchResultColor4 = new Color();
        public Color SynonymSearchResultColor4
        {
            get
            {
                return _synonymSearchResultColor4;
            }
            set
            {
                _synonymSearchResultColor4 = value;
                OnPropertyChanged("SynonymSearchResultColor4");
            }
        }


        private Color _synonymSearchResultColor5 = new Color();
        public Color SynonymSearchResultColor5
        {
            get
            {
                return _synonymSearchResultColor5;
            }
            set
            {
                _synonymSearchResultColor5 = value;
                OnPropertyChanged("SynonymSearchResultColor5");
            }
        }

        private Color _synonymSearchResultColor6 = new Color();
        public Color SynonymSearchResultColor6
        {
            get
            {
                return _synonymSearchResultColor6;
            }
            set
            {
                _synonymSearchResultColor6 = value;
                OnPropertyChanged("SynonymSearchResultColor6");
            }
        }

        private Color _synonymSearchResultColor7 = new Color();
        public Color SynonymSearchResultColor7
        {
            get
            {
                return _synonymSearchResultColor7;
            }
            set
            {
                _synonymSearchResultColor7 = value;
                OnPropertyChanged("SynonymSearchResultColor7");
            }
        }


        private Color _synonymSearchResultColor8 = new Color();
        public Color SynonymSearchResultColor8
        {
            get
            {
                return _synonymSearchResultColor8;
            }
            set
            {
                _synonymSearchResultColor8 = value;
                OnPropertyChanged("SynonymSearchResultColor8");
            }
        }


        private Color _synonymSearchResultColor9 = new Color();
        public Color SynonymSearchResultColor9
        {
            get
            {
                return _synonymSearchResultColor9;
            }
            set
            {
                _synonymSearchResultColor9 = value;
                OnPropertyChanged("SynonymSearchResultColor9");
            }
        }

        private Color _synonymSearchResultColor10 = new Color();
        public Color SynonymSearchResultColor10
        {
            get
            {
                return _synonymSearchResultColor10;
            }
            set
            {
                _synonymSearchResultColor10 = value;
                OnPropertyChanged("SynonymSearchResultColor10");
            }
        }

        private Color _synonymSearchResultFontColor = new Color();
        public Color SynonymSearchResultFontColor
        {
            get
            {
                return _synonymSearchResultFontColor;
            }
            set
            {
                _synonymSearchResultFontColor = value;
                OnPropertyChanged("SynonymSearchResultFontColor");
            }
        }

        private string _searchResultMargin = null;
        public string SearchResultMargin
        {
            get
            {
                return _searchResultMargin;
            }
            set
            {
                _searchResultMargin = value;
                OnPropertyChanged("SearchResultMargin");
            }
        }

        private string _searchResultCount = null;
        public string SearchResultCount
        {
            get
            {
                return _searchResultCount;
            }
            set
            {
                _searchResultCount = value;
                OnPropertyChanged("SearchResultCount");
            }
        }


        #endregion


        #endregion

        #endregion

        #region method

        /// <summary>コンストラクタ</summary>
        public SettingWindowVM()
        {
            Initialize();
        }

        /// <summary>初期化処理</summary>
        private void Initialize()
        {
            Command_Ok = new CommandBase(ExecuteOk, null);
            Command_Cancel = new CommandBase(ExecuteCancel, null);
            Command_Apply = new CommandBase(ExecuteApply, null);
            Command_ResetToDefault = new CommandBase(ExecuteResetToDefault, null);



            //Model.FileAccessor.GetFileAccessor.LoadSettingFile();
        }

        /// <summary>OKボタン押下時処理</summary>
        /// <param name="parameter"></param>
        /// <remarks>ExecuteApply実行後にCloseWindowを行うのみ</remarks>
        private void ExecuteOk(object parameter)
        {
            ExecuteApply(parameter);
            CloseSettingWindow();
        }

        /// <summary>キャンセルボタン押下時処理</summary>
        /// <param name="parameter"></param>
        private void ExecuteCancel(object parameter)
        {
            CloseSettingWindow();
        }

        /// <summary>適用ボタン押下時処理</summary>
        /// <param name="parameter"></param>
        private void ExecuteApply(object parameter)
        {

            #region GeneralSetting

            Settings.GeneralSetting generalSetting = new Settings.GeneralSetting
            {
                WrappingText = this.WrappingText,
                ShowingLineCount = this.ShowingLineCount,
                ShowingLineNumber = this.ShowingLineNumber,
                ShowingWordCount = this.ShowingWordCount,
                ShowingNewLine = this.ShowingNewLine,
                ShowingTab = this.ShowingTab,
                ShowingSpace = this.ShowingSpace,
                FontSize = this.FontSize,
                FontColor = this.FontColor.ToString(),
                MainFontName = this.JapaneseFontName,
                SubFontName = this.EnglishFontName
            };

            Model.FileAccessor.GetFileAccessor.SaveSettingFile(CommonLibrary.Define.SETTING_FILENAME_GENERAL, generalSetting, typeof(Settings.GeneralSetting));

            #endregion

            #region AdvancedSetting

            int resultDisplayCount = 0;
            if (int.TryParse(SearchResultDisplayCount, out resultDisplayCount) == false)
            {
                resultDisplayCount = 100;//todo 規定値
            }

            Settings.AdvancedSetting advancedSetting = new Settings.AdvancedSetting
            {
                LogLevel = ConvertDoubleToLogLevel(LogOutputLevel),
                SpeedUpSearch = UseFastSearch,
                LogRetentionDays = Convert.ToInt32(LogRetentionDays),
                SynonymSearchResultDisplayCount = resultDisplayCount,
                TargetFileExtensionList = GetTargetExtensionsList()
            };

            Model.FileAccessor.GetFileAccessor.SaveSettingFile(CommonLibrary.Define.SETTING_FILENAME_ADVANCED, advancedSetting, typeof(Settings.AdvancedSetting));

            #endregion
        }


        private void ExecuteResetToDefault(object parameter)
        {
            if (parameter == null)
            {
                return;
            }

            if (Enum.IsDefined(typeof(CommonLibrary.SettingResetKind), parameter))
            {
                var a = (CommonLibrary.SettingResetKind)Enum.ToObject(typeof(CommonLibrary.SettingResetKind), parameter);
                //todo:下流処理→Model
            }
            else
            {
                // 現状、想定していない
                CommonLibrary.Log.Logger.Error(CLASS_NAME, "ExecuteResetToDefault", "parameter is null!");
                return;
            }

            // todo:タブ毎に分かれた処理にすること
        }

        /// <summary>設定ウィンドウを閉じます</summary>
        private void CloseSettingWindow()
        {
            Model.Manager.WindowManager.CloseSubWindow(CommonLibrary.Define.SubWindowName.SettingWindow);
        }

        private List<string> GetTargetExtensionsList()
        {
            List<string> targets = new List<string>();

            if (IsTxtTarget)
            {
                targets.Add("txt");//todo:固定値化
            }

            return targets;
        }

        private CommonLibrary.LogLevel ConvertDoubleToLogLevel(double logLevel)
        {
            int intLogLevel = Convert.ToInt32(logLevel);

            if (Enum.IsDefined(typeof(CommonLibrary.LogLevel), intLogLevel))
            {
                return (CommonLibrary.LogLevel)Enum.ToObject(typeof(CommonLibrary.LogLevel), intLogLevel);
            }

            // 変換不可な値だった場合、規定値を返すようにする
            // todo:error log
            return CommonLibrary.LogLevel.INFO;
        }


        /// <summary>ログ出力レベル表記をスライダーバードラッグにあわせて更新します</summary>
        /// <param name="logLevel"></param>
        private void UpdateLogLevelVisible(CommonLibrary.LogLevel logLevel)
        {
            switch (logLevel)
            {
                case CommonLibrary.LogLevel.DEBUG:
                    DebugVisible = Visibility.Visible;
                    InfoVisible = Visibility.Visible;
                    WarnVisible = Visibility.Visible;
                    ErrorVisible = Visibility.Visible;
                    FatalVisible = Visibility.Visible;
                    break;
                case CommonLibrary.LogLevel.INFO:
                    DebugVisible = Visibility.Hidden;
                    InfoVisible = Visibility.Visible;
                    WarnVisible = Visibility.Visible;
                    ErrorVisible = Visibility.Visible;
                    FatalVisible = Visibility.Visible;
                    break;
                case CommonLibrary.LogLevel.WARN:
                    DebugVisible = Visibility.Hidden;
                    InfoVisible = Visibility.Hidden;
                    WarnVisible = Visibility.Visible;
                    ErrorVisible = Visibility.Visible;
                    FatalVisible = Visibility.Visible;
                    break;
                case CommonLibrary.LogLevel.ERROR:
                    DebugVisible = Visibility.Hidden;
                    InfoVisible = Visibility.Hidden;
                    WarnVisible = Visibility.Hidden;
                    ErrorVisible = Visibility.Visible;
                    FatalVisible = Visibility.Visible;
                    break;
                case CommonLibrary.LogLevel.FATAL:
                    DebugVisible = Visibility.Hidden;
                    InfoVisible = Visibility.Hidden;
                    WarnVisible = Visibility.Hidden;
                    ErrorVisible = Visibility.Hidden;
                    FatalVisible = Visibility.Visible;
                    break;
                default: // 想定していないが、とりあえず全部表示させておく？
                    //todo:ErrorLog
                    DebugVisible = Visibility.Visible;
                    InfoVisible = Visibility.Visible;
                    WarnVisible = Visibility.Visible;
                    ErrorVisible = Visibility.Visible;
                    FatalVisible = Visibility.Visible;
                    break;
            }
        }

        private void Test_FontGet()
        {
            //todo:起動時に一度呼ぶだけで良いので、設定ファイルの読み込み箇所とかに移動させるべき
            // 日本語フォントを日本語で表示したいので、現在動いている環境の言語を取得する
            //todo:WinwowのLanguageに、このlanguageを指定する必要がある
            System.Windows.Markup.XmlLanguage language = System.Windows.Markup.XmlLanguage.GetLanguage
                (System.Threading.Thread.CurrentThread.CurrentCulture.Name);

            // 使える全言語を取得
            var fonts = System.Windows.Media.Fonts.SystemFontFamilies.Select
                 (i => new FontInfo() { FontFamily = i, FontName = i.Source });

            // このままだと日本語で表示してくれないので、日本語のものはこちらで取得して入れ込み、表示してやる
            fonts.Select(i => i.FontName = i.FontFamily.FamilyNames
                 .FirstOrDefault(j => j.Key == language).Value ?? i.FontFamily.Source).ToArray();

            //return fonts;

        }

        private class FontInfo
        {
            internal System.Windows.Media.FontFamily FontFamily { get; set; }
            internal string FontName { get; set; }
        }

        #endregion

    }
}
