using SynonyMe.CommonLibrary;
using SynonyMe.CommonLibrary.Log;
using SynonyMe.Settings;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynonyMe.Model.Manager
{
    internal class SettingManager
    {
        #region singleton

        private static readonly SettingManager _settingManager = new SettingManager();

        /// <summary>設定ファイル関連管理クラスを取得します</summary>
        internal static SettingManager GetSettingManager
        {
            get
            {
                return _settingManager;
            }
        }

        /// <summary>シングルトン担保</summary>
        /// <remarks>起動時(インスタンス生成時)に、必ず全設定ファイルを読み込みます</remarks>
        private SettingManager()
        {
            LoadAllSettings();
        }

        #endregion

        #region field

        /// <summary>本exeが扱う全設定情報を管理します</summary>
        private readonly ConcurrentDictionary<Type, object> _settingDictionary = new ConcurrentDictionary<Type, object>();

        #endregion

        #region event:todo

        /// <summary>高度な設定変更時に発火するイベントハンドラ</summary>
        internal event EventHandler<Events.SettingChangedEventArgs> AdvancedSettingChangedEvent = delegate { };

        /// <summary>一般設定変更時に発火するイベントハンドラ</summary>
        internal event EventHandler<Events.SettingChangedEventArgs> GeneralSettingChangedEvent = delegate { };

        /// <summary>検索・類語検索設定変更時に発火するイベントハンドラ</summary>
        internal event EventHandler<Events.SettingChangedEventArgs> SearchAndSynonymSettingChangedEvent = delegate { };


        /// <summary>設定の変更を通知します</summary>
        /// todo:SettingManagerに設定ファイル読み書きが移管されたらこのあたり作成する
        internal void NotifySettingChanged(CommonLibrary.SettingResetKind kind)
        {
            switch (kind)
            {
                case CommonLibrary.SettingResetKind.AdvancedSetting:
                    AdvancedSettingChangedEvent(this,
                        new Events.SettingChangedEventArgs(typeof(AdvancedSetting), GetSettingManager.GetSetting(typeof(AdvancedSetting))));
                    break;

                case CommonLibrary.SettingResetKind.GeneralSetting:
                    GeneralSettingChangedEvent(this,
                        new Events.SettingChangedEventArgs(typeof(GeneralSetting), GetSettingManager.GetSetting(typeof(GeneralSetting))));
                    break;


                case CommonLibrary.SettingResetKind.SearchAndSynonymSetting:

                    break;

                case CommonLibrary.SettingResetKind.AllReset:

                    break;

                default:
                    //todo:error log
                    break;
            }

            //SettingChangedEventHandlerに、設定変更に対応するべきメソッドを全て登録させる
            //設計イメージとしては、このメソッド内で最初にイベント引数を作成しておく
            //Updateが必要な設定ファイル情報を粛々と入れていき、この中で発火させる。引数で受け取った方が良いかもしれない。
            //登録されたメソッドが、自分自身で更新要不要を判断する
            AdvancedSettingChangedEvent(this, new Events.SettingChangedEventArgs(null));
        }

        #endregion

        private const string CLASS_NAME = "SettingManager";

        /// <summary>設定一覧に設定ファイル情報を追加します</summary>
        /// <param name="type">設定ファイル種別</param>
        /// <param name="setting">設定ファイルインスタンス</param>
        /// <returns>true:成功, false:失敗</returns>
        private bool AddSetting(Type type, object setting)
        {
            if (_settingDictionary.TryAdd(type, setting) == false)
            {
                //todo:log
                return false;
            }

            return true;
        }

        /// <summary>指定した設定情報を取得します</summary>
        /// <param name="target"></param>
        /// <returns></returns>
        internal object GetSetting(Type target)
        {
            object setting;
            if (_settingDictionary.TryGetValue(target, out setting) == false)
            {
                //todo:log
                return null;
            }

            return setting;
        }

        /// <summary>管理下にある設定情報を更新、また存在していない場合は登録します</summary>
        /// <param name="type"></param>
        /// <param name="setting"></param>
        /// <returns></returns>
        internal bool UpdateOrAddSetting(Type type, object setting)
        {
            if (_settingDictionary.ContainsKey(type))
            {
                if (_settingDictionary.TryUpdate(type, setting, _settingDictionary[type]) == false)
                {
                    //todo:log
                    return false;
                }
            }
            else
            {
                //todo;log
                return AddSetting(type, setting);
            }

            return true;
        }

        private void LoadAllSettings()
        {
            // 設定ファイル読み込みに時間がかかることを想定し、マルチスレッドで行わせる
            Task task = Task.Run(() =>
            {
                LoadGeneralSetting();
                LoadSearchAndSynonymSetting();
                LoadAdvancedSetting();
            });

            //LoadGeneralSetting();
            //LoadSearchAndSynonymSetting();
            //LoadAdvancedSetting();
        }


        internal bool LoadAdvancedSetting()
        {
            AdvancedSetting advancedSetting = FileAccessor.GetFileAccessor.LoadSettingFile<AdvancedSetting>(Define.SETTING_FILENAME_ADVANCED);
            if (advancedSetting == null)
            {
                Logger.Error(CLASS_NAME, "LoadAdvancedSetting", $"Load AdvancedSetting failed. FileName:[{Define.SETTING_FILENAME_ADVANCED}]");
                advancedSetting = new AdvancedSetting()
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

            if (AddSetting(typeof(AdvancedSetting), advancedSetting) == false)
            {
                //todo:log
                return false;
            }

            return true;
        }


        internal bool LoadGeneralSetting()
        {
            GeneralSetting generalSetting = FileAccessor.GetFileAccessor.LoadSettingFile<GeneralSetting>(Define.SETTING_FILENAME_GENERAL);
            if (generalSetting == null)
            {
                Logger.Error(CLASS_NAME, "LoadSettings", $"Load GeneralSetting.xml failed. Filename:[{Define.SETTING_FILENAME_GENERAL}]");
                generalSetting = new GeneralSetting()
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

            if (AddSetting(typeof(GeneralSetting), generalSetting) == false)
            {
                //todo:log
                return false;
            }

            return true;
        }



        internal bool LoadSearchAndSynonymSetting()
        {
            SearchAndSynonymSetting searchAndSynonymSetting = FileAccessor.GetFileAccessor.LoadSettingFile<SearchAndSynonymSetting>(Define.SETTING_FILENAME_SEARCH);
            if (searchAndSynonymSetting == null)
            {
                Logger.Error(CLASS_NAME, "LoadSearchAndSynonymSetting", $"Load SearchAndSynonymSetting.xml failed. FileName:[{Define.SETTING_FILENAME_SEARCH}]");
                string black = "#FF000000";
                string transparent = "#00FFFFFF";

                searchAndSynonymSetting = new SearchAndSynonymSetting()
                {
                    SearchResultBackGroundColor = transparent, // 透明
                    SearchResultDisplayCount = 100,
                    SearchResultFontColor = black, // 黒
                    SearchResultFontColorKind = FontColorKind.Auto,
                    SearchResultMargin = 10,
                    SynonymSearchFontColor = black,
                    SynonymSearchFontColorKind = FontColorKind.Auto,
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

            if (AddSetting(typeof(SearchAndSynonymSetting), searchAndSynonymSetting) == false)
            {
                //todo:log
                return false;
            }

            return true;
        }
    }
}
