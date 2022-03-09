using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using SynonyMe.Settings;
using SynonyMe.CommonLibrary;
using SynonyMe.Model;
using SynonyMe.CommonLibrary.Log;

namespace SynonyMe.ViewModel
{
    public class SettingWindowVM : ViewModelBase
    {
        #region field

        private const string CLASS_NAME = "SettingWindowVM";

        private GeneralSetting _generalSetting = null;

        private SearchAndSynonymSetting _searchAndSynonymSetting = null;

        private AdvancedSetting _advancedSetting = null;

        private Model.SettingWindowModel _model = null;

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

        #region Tab_GeneralSettings

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

        private string _mainFontName = "Meiryo";
        public string MainFontName
        {
            get
            {
                return _mainFontName;
            }
            set
            {
                if (_mainFontName != value)
                {
                    _mainFontName = value;
                    OnPropertyChanged("MainFontName");
                }
            }
        }

        private string _subFontName = "Consolas";
        public string SubFontName
        {
            get
            {
                return _subFontName;
            }
            set
            {
                if (_subFontName != value)
                {
                    _subFontName = value;
                    OnPropertyChanged("SubFontName");
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


        private Color _synonymSearchResultColor5 = new Color()
        {
            R = 123,
            G = 222,
            B = 30,
            A = 255
        };
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



        private string _searchResultMargin = "10";
        public string SearchResultMargin
        {
            get
            {
                return _searchResultMargin;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }
                if (System.Text.RegularExpressions.Regex.IsMatch(value, Define.REGEX_NUMBER_ONLY) == false)
                {
                    return;
                }

                _searchResultMargin = value;
                OnPropertyChanged("SearchResultMargin");
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
                if (System.Text.RegularExpressions.Regex.IsMatch(value, Define.REGEX_NUMBER_ONLY) == false)
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
            _model = new SettingWindowModel(this);

            Command_Ok = new CommandBase(ExecuteOk, null);
            Command_Cancel = new CommandBase(ExecuteCancel, null);
            Command_Apply = new CommandBase(ExecuteApply, null);
            Command_ResetToDefault = new CommandBase(ExecuteResetToDefault, null);

            LoadSettings();

            ApplyAllSettings();
        }

        /// <summary>全設定ファイルを読み込みます</summary>
        private void LoadSettings()
        {
            _generalSetting = FileAccessor.GetFileAccessor.LoadSettingFile<GeneralSetting>(Define.SETTING_FILENAME_GENERAL);
            if (_generalSetting == null)
            {
                Logger.Error(CLASS_NAME, "LoadSettings", $"Load GeneralSetting.xml failed. Filename:[{Define.SETTING_FILENAME_GENERAL}]");
                _generalSetting = new GeneralSetting()
                {
                    FontColor = "#FF000000", // 黒
                    FontSize = 12,
                    MainFontName = "Meiryo",
                    SubFontName = "Consolas",
                    WrappingText = false,
                    ShowingLineCount = true,
                    ShowingLineNumber = true,
                    ShowingNewLine = false,
                    ShowingSpace = false,
                    ShowingTab = false,
                    ShowingWordCount = true
                };
            }

            _searchAndSynonymSetting = FileAccessor.GetFileAccessor.LoadSettingFile<SearchAndSynonymSetting>(Define.SETTING_FILENAME_SEARCH);
            if (_searchAndSynonymSetting == null)
            {
                Logger.Error(CLASS_NAME, "LoadSettings", $"Load SearchAndSynonymSetting.xml failed. FileName:[{Define.SETTING_FILENAME_SEARCH}]");
                string black = "#FF000000";
                string transparent = "#00FFFFFF";

                _searchAndSynonymSetting = new SearchAndSynonymSetting()
                {
                    SearchResultBackGroundColor = transparent, // 透明
                    SearchResultDisplayCount = 100,
                    SearchResultFontColor = black, // 黒
                    SearchResultMargin = 10,
                    SynonymSearchFontColor = black,
                    SynonymSearchResultColor1 = transparent, // 異常なので透明にしておく、todo:規定値どうするか
                    SynonymSearchResultColor2 = transparent,
                    SynonymSearchResultColor3 = transparent,
                    SynonymSearchResultColor4 = transparent,
                    SynonymSearchResultColor5 = transparent,
                    SynonymSearchResultColor6 = transparent,
                    SynonymSearchResultColor7 = transparent,
                    SynonymSearchResultColor8 = transparent,
                    SynonymSearchResultColor9 = transparent,
                    SynonymSearchResultColor10 = transparent
                };
            }

            _advancedSetting = FileAccessor.GetFileAccessor.LoadSettingFile<AdvancedSetting>(Define.SETTING_FILENAME_ADVANCED);
            if (_advancedSetting == null)
            {
                Logger.Error(CLASS_NAME, "LoadSettings", $"Load AdvancedSetting failed. FileName:[{Define.SETTING_FILENAME_ADVANCED}]");
                _advancedSetting = new AdvancedSetting()
                {
                    LogRetentionDays = 30,
                    LogLevel = LogLevel.INFO,
                    SpeedUpSearch = false,
                    TargetFileExtensionList = new List<string>() //todo:初期値はこれで良いか？
                    {
                        "txt"
                    }
                };
            }
        }

        /// <summary>全設定を更新・適用します</summary>
        private void ApplyAllSettings()
        {
            // LoadSettingsで必ず値を代入しているはずなので、nullチェックだけして異常ならreturnしてしまう
            // この時点でexe動作の正常性を担保できていない(内部処理でnewしているのにnullっているため)
            if (_generalSetting == null)
            {
                Logger.Fatal(CLASS_NAME, "ApplySettings", "_generalSetting is null!");
                return;
            }

            WrappingText = _generalSetting.WrappingText;
            ShowingLineCount = _generalSetting.ShowingLineCount;
            ShowingLineNumber = _generalSetting.ShowingLineNumber;
            ShowingWordCount = _generalSetting.ShowingWordCount;
            ShowingNewLine = _generalSetting.ShowingNewLine;
            ShowingTab = _generalSetting.ShowingTab;
            ShowingSpace = _generalSetting.ShowingSpace;
            FontColor = ConvertStringToColor(_generalSetting.FontColor);
            MainFontName = _generalSetting.MainFontName;
            SubFontName = _generalSetting.SubFontName;

            if (_searchAndSynonymSetting == null)
            {
                Logger.Fatal(CLASS_NAME, "ApplySettings", "_searchAndSynonymSetting is null!");
                return;
            }

            SearchResultBackGround = ConvertStringToColor(_searchAndSynonymSetting.SearchResultBackGroundColor);
            SearchResultFontColor = ConvertStringToColor(_searchAndSynonymSetting.SearchResultFontColor);
            SynonymSearchResultColor1 = ConvertStringToColor(_searchAndSynonymSetting.SynonymSearchResultColor1);
            SynonymSearchResultColor2 = ConvertStringToColor(_searchAndSynonymSetting.SynonymSearchResultColor2);
            SynonymSearchResultColor3 = ConvertStringToColor(_searchAndSynonymSetting.SynonymSearchResultColor3);
            SynonymSearchResultColor4 = ConvertStringToColor(_searchAndSynonymSetting.SynonymSearchResultColor4);
            SynonymSearchResultColor5 = ConvertStringToColor(_searchAndSynonymSetting.SynonymSearchResultColor5);
            SynonymSearchResultColor6 = ConvertStringToColor(_searchAndSynonymSetting.SynonymSearchResultColor6);
            SynonymSearchResultColor7 = ConvertStringToColor(_searchAndSynonymSetting.SynonymSearchResultColor7);
            SynonymSearchResultColor8 = ConvertStringToColor(_searchAndSynonymSetting.SynonymSearchResultColor8);
            SynonymSearchResultColor9 = ConvertStringToColor(_searchAndSynonymSetting.SynonymSearchResultColor9);
            SynonymSearchResultColor10 = ConvertStringToColor(_searchAndSynonymSetting.SynonymSearchResultColor10);
            SynonymSearchResultFontColor = ConvertStringToColor(_searchAndSynonymSetting.SynonymSearchFontColor);
            SearchResultMargin = _searchAndSynonymSetting.SearchResultMargin.ToString();
            SearchResultDisplayCount = _searchAndSynonymSetting.SearchResultDisplayCount.ToString();

            if (_advancedSetting == null)
            {
                Logger.Fatal(CLASS_NAME, "ApplySettings", "_advancedSetting is null!");
                return;
            }

            LogOutputLevel = (double)_advancedSetting.LogLevel; //todo:安全なキャスト
            LogRetentionDays = _advancedSetting.LogRetentionDays;
            UseFastSearch = _advancedSetting.SpeedUpSearch;
            if (_advancedSetting.TargetFileExtensionList.Contains("txt")) //todo:リスト検索方法
            {
                IsTxtTarget = true;
            }

            //todo:プロパティに値割り当て
        }

        /// <summary>#AARRGGBB形式の文字列をColor構造体に変換します</summary>
        /// <param name="source">#AARRGGBBであること</param>
        /// <returns>文字列異常は透明(#00FFFFFF),変換失敗は対応チャネルをFF</returns>
        private Color ConvertStringToColor(string source)
        {
            // [#AARRGGBB]文字列形式であるかを確認
            if (string.IsNullOrEmpty(source) ||
               source.Length != "#AARRGGBB".Length)
            {
                Logger.Error(CLASS_NAME, "ConvertStringToColor", $"source is incorrect! value[{source}]");
                // 背景色をとりあえず透明で返しておくことにする
                return Colors.Transparent;
            }

            string stringAValue = source.Substring(1, 2);
            string stringRValue = source.Substring(3, 2);
            string stringGValue = source.Substring(5, 2);
            string stringBValue = source.Substring(7, 2);

            byte byteAValue, byteRValue, byteGValue, byteBValue;

            if (byte.TryParse(stringAValue, System.Globalization.NumberStyles.HexNumber, null, out byteAValue) == false)
            {
                Logger.Error(CLASS_NAME, "ConvertStringToColor", $"AValue is invalid.[{stringAValue}]");
                byteAValue = 255;
            }
            if (byte.TryParse(stringRValue, System.Globalization.NumberStyles.HexNumber, null, out byteRValue) == false)
            {
                Logger.Error(CLASS_NAME, "ConvertStringToColor", $"RValue is invalid.[{stringRValue}]");
                byteRValue = 255;
            }
            if (byte.TryParse(stringGValue, System.Globalization.NumberStyles.HexNumber, null, out byteGValue) == false)
            {
                Logger.Error(CLASS_NAME, "ConvertStringToColor", $"GValue is invalid.[{stringGValue}]");
                byteGValue = 255;
            }
            if (byte.TryParse(stringBValue, System.Globalization.NumberStyles.HexNumber, null, out byteBValue) == false)
            {
                Logger.Error(CLASS_NAME, "ConvertStringToColor", $"GValue is invalid.[{stringBValue}]");
                byteBValue = 255;
            }

            return new Color()
            {
                A = byteAValue,
                R = byteRValue,
                G = byteGValue,
                B = byteBValue
            };
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
            //todo:メソッド切り出し
            #region GeneralSetting

            int convFontSize = 0;
            if (int.TryParse(FontSize, out convFontSize))
            {
                //todo:log. default
            }

            GeneralSetting generalSetting = new GeneralSetting
            {
                WrappingText = this.WrappingText,
                ShowingLineCount = this.ShowingLineCount,
                ShowingLineNumber = this.ShowingLineNumber,
                ShowingWordCount = this.ShowingWordCount,
                ShowingNewLine = this.ShowingNewLine,
                ShowingTab = this.ShowingTab,
                ShowingSpace = this.ShowingSpace,
                FontSize = convFontSize,
                FontColor = this.FontColor.ToString(),
                MainFontName = this.MainFontName,
                SubFontName = this.SubFontName
            };

            FileAccessor.GetFileAccessor.SaveSettingFile(Define.SETTING_FILENAME_GENERAL, generalSetting, typeof(GeneralSetting));

            #endregion

            #region SearchAndSynonymSetting

            int margin = 0;
            if (int.TryParse(SearchResultMargin, out margin) == false)
            {
                //todo:ログ出し、規定値
            }

            int resultCount = 0;
            if (int.TryParse(SearchResultDisplayCount, out resultCount) == false)
            {
                //todo:ログ出し、規定値
            }

            SearchAndSynonymSetting searchAndSynonymSetting = new SearchAndSynonymSetting()
            {
                SearchResultBackGroundColor = SearchResultBackGround.ToString(),
                SearchResultFontColor = SearchResultFontColor.ToString(),

                SynonymSearchResultColor1 = SynonymSearchResultColor1.ToString(),
                SynonymSearchResultColor2 = SynonymSearchResultColor2.ToString(),
                SynonymSearchResultColor3 = SynonymSearchResultColor3.ToString(),
                SynonymSearchResultColor4 = SynonymSearchResultColor4.ToString(),
                SynonymSearchResultColor5 = SynonymSearchResultColor5.ToString(),
                SynonymSearchResultColor6 = SynonymSearchResultColor6.ToString(),
                SynonymSearchResultColor7 = SynonymSearchResultColor7.ToString(),
                SynonymSearchResultColor8 = SynonymSearchResultColor8.ToString(),
                SynonymSearchResultColor9 = SynonymSearchResultColor9.ToString(),
                SynonymSearchResultColor10 = SynonymSearchResultColor10.ToString(),

                SynonymSearchFontColor = SynonymSearchResultFontColor.ToString(),
                SearchResultMargin = margin,
                SearchResultDisplayCount = resultCount
            };

            FileAccessor.GetFileAccessor.SaveSettingFile(Define.SETTING_FILENAME_SEARCH, searchAndSynonymSetting, typeof(SearchAndSynonymSetting));

            #endregion

            #region AdvancedSetting

            int resultDisplayCount = 0;
            if (int.TryParse(SearchResultDisplayCount, out resultDisplayCount) == false)
            {
                resultDisplayCount = 100;//todo 規定値
            }

            AdvancedSetting advancedSetting = new AdvancedSetting
            {
                LogLevel = ConvertDoubleToLogLevel(LogOutputLevel),
                SpeedUpSearch = UseFastSearch,
                LogRetentionDays = Convert.ToInt32(LogRetentionDays),
                TargetFileExtensionList = GetTargetExtensionsList()
            };

            FileAccessor.GetFileAccessor.SaveSettingFile(Define.SETTING_FILENAME_ADVANCED, advancedSetting, typeof(AdvancedSetting));

            #endregion

            //todo:完了ダイアログ表示
        }

        /// <summary>表示されているタブをデフォルトにリセットします</summary>
        /// <param name="parameter"></param>
        private void ExecuteResetToDefault(object parameter)
        {
            if (parameter == null)
            {
                return;
            }

            if (Enum.IsDefined(typeof(SettingResetKind), parameter))
            {
                var a = (SettingResetKind)Enum.ToObject(typeof(SettingResetKind), parameter);
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

        /// <summary>全類語グループおよび類語を削除します</summary>
        private void DeleteAllSynonymGroupsAndWords()
        {
            //tood:確認ダイアログ
        }

        /// <summary>全ログファイルを削除します</summary>
        private void DeleteAllLogFiles()
        {
            //todo:確認ダイアログ
        }

        /// <summary>全設定を初期値にリセットします</summary>
        private void ResetAllSettingsToDefault()
        {
            // todo:確認ダイアログ
        }

        /// <summary>設定ウィンドウを閉じます</summary>
        private void CloseSettingWindow()
        {
            Model.Manager.WindowManager.CloseSubWindow(Define.SubWindowName.SettingWindow);
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

        private LogLevel ConvertDoubleToLogLevel(double logLevel)
        {
            int intLogLevel = Convert.ToInt32(logLevel);

            if (Enum.IsDefined(typeof(LogLevel), intLogLevel))
            {
                return (LogLevel)Enum.ToObject(typeof(LogLevel), intLogLevel);
            }

            // 変換不可な値だった場合、規定値を返すようにする
            // todo:error log
            return LogLevel.INFO;
        }


        /// <summary>ログ出力レベル表記をスライダーバードラッグにあわせて更新します</summary>
        /// <param name="logLevel"></param>
        private void UpdateLogLevelVisible(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.DEBUG:
                    DebugVisible = Visibility.Visible;
                    InfoVisible = Visibility.Visible;
                    WarnVisible = Visibility.Visible;
                    ErrorVisible = Visibility.Visible;
                    FatalVisible = Visibility.Visible;
                    break;
                case LogLevel.INFO:
                    DebugVisible = Visibility.Hidden;
                    InfoVisible = Visibility.Visible;
                    WarnVisible = Visibility.Visible;
                    ErrorVisible = Visibility.Visible;
                    FatalVisible = Visibility.Visible;
                    break;
                case LogLevel.WARN:
                    DebugVisible = Visibility.Hidden;
                    InfoVisible = Visibility.Hidden;
                    WarnVisible = Visibility.Visible;
                    ErrorVisible = Visibility.Visible;
                    FatalVisible = Visibility.Visible;
                    break;
                case LogLevel.ERROR:
                    DebugVisible = Visibility.Hidden;
                    InfoVisible = Visibility.Hidden;
                    WarnVisible = Visibility.Hidden;
                    ErrorVisible = Visibility.Visible;
                    FatalVisible = Visibility.Visible;
                    break;
                case LogLevel.FATAL:
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
            var fonts = Fonts.SystemFontFamilies.Select
                 (i => new FontInfo() { FontFamily = i, FontName = i.Source });

            // このままだと日本語で表示してくれないので、日本語のものはこちらで取得して入れ込み、表示してやる
            fonts.Select(i => i.FontName = i.FontFamily.FamilyNames
                 .FirstOrDefault(j => j.Key == language).Value ?? i.FontFamily.Source).ToArray();

            //return fonts;

        }

        private class FontInfo
        {
            internal FontFamily FontFamily { get; set; }
            internal string FontName { get; set; }
        }

        #endregion

    }
}
