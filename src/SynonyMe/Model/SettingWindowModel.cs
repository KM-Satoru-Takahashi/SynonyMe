using SynonyMe.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SynonyMe.Model.Manager;
using SynonyMe.CommonLibrary.Log;
using SynonyMe.CommonLibrary;

namespace SynonyMe.Model
{
    /// <summary>設定ウィンドウの内部処理を担います</summary>
    internal class SettingWindowModel
    {
        private const string CLASS_NAME = "SettingWindowModel";

        private ViewModel.SettingWindowVM _viewModel = null;

        internal SettingWindowModel(ViewModel.SettingWindowVM vm)
        {
            _viewModel = vm;
        }

        internal GeneralSetting GetGeneralSetting()
        {
            object setting = SettingManager.GetSettingManager.GetSetting(typeof(GeneralSetting));
            if (setting == null)
            {
                Logger.Warn(CLASS_NAME, "GetGeneralSetting", "setting is null!");
                //todo:デフォルト値の定数化
                setting = new GeneralSetting()
                {
                    FontColor = "#FF000000", // 黒
                    FontSize = 12.0,
                    MainFontName = "Meiryo",
                    SubFontName = "Consolas",
                    WrappingText = false,
                    ShowingLineCount = true,
                    ShowingNumberOfLines = true,
                    ShowingNewLine = false,
                    ShowingSpace = false,
                    ShowingTab = false,
                    ShowingWordCount = true
                };
            }

            return setting as GeneralSetting;
        }

        internal AdvancedSetting GetAdvancedSetting()
        {
            object setting = SettingManager.GetSettingManager.GetSetting(typeof(AdvancedSetting));
            if (setting == null)
            {
                Logger.Error(CLASS_NAME, "GetAdvancedSetting", $"Get AdvancedSetting failed. FileName:[{Define.SETTING_FILENAME_ADVANCED}]");
                setting = new AdvancedSetting()
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

            return setting as AdvancedSetting;
        }

        /// <summary>
        /// 検索・類語検索設定を取得します
        /// </summary>
        /// <returns></returns>
        /// <remarks>todo:設定情報取得周り、nullならもうどうしようもないからログ出ししてnullをreturnとかさせるべき
        /// また、プロパティ値の設定はModel側で完結させるべき</remarks>
        internal SearchAndSynonymSetting GetSearchAndSynonymSetting()
        {
            object setting = SettingManager.GetSettingManager.GetSetting(typeof(SearchAndSynonymSetting));
            if (setting == null)
            {
                Logger.Error(CLASS_NAME, "GetSearchAndSynonymSetting", $"Get SearchAndSynonymSetting.xml failed. FileName:[{Define.SETTING_FILENAME_SEARCH}]");
                string black = "#FF000000";
                string transparent = "#00FFFFFF";

                setting = new SearchAndSynonymSetting()
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

            return setting as SearchAndSynonymSetting;

        }

    }
}
