using SynonyMe.CommonLibrary.Log;
using SynonyMe.Model.Manager;
using SynonyMe.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynonyMe.Model.SettingWindow
{
    /// <summary>一般設定[GeneralSetting]に関する情報と操作を有します</summary>
    internal class GeneralSettingModel
    {
        /// <summary>呼び出し元</summary>
        private SettingWindowModel _parent = null;

        private const string CLASS_NAME = "GeneralSettingModel";

        /// <summary>コンストラクタ</summary>
        /// <param name="model">SettingWindowModel</param>
        internal GeneralSettingModel(SettingWindowModel model)
        {
            if (model == null)
            {
                //todo:error log
                return;
            }

            _parent = model;
        }

        /// <summary>一般設定情報を取得します</summary>
        /// <returns>異常時:null</returns>
        private GeneralSetting GetGeneralSetting()
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
                    ShowingWordCount = true//todo:Main, SubFont
                };
            }

            return setting as GeneralSetting;
        }

        internal void ApplyGeneralSetting()
        {

            // Model側で値は必ずセットされているはずなので、nullチェックだけして異常ならreturnしてしまう
            // この時点でexe動作の正常性を担保できていない(内部処理でnewしているのにnullっているため)
            GeneralSetting generalSetting = GetGeneralSetting();
            if (generalSetting == null)
            {
                Logger.Fatal(CLASS_NAME, "ApplyGeneralSetting", "generalSetting is null!");
                return;
            }

            _parent.ViewModel.WrappingText = generalSetting.WrappingText;
            _parent.ViewModel.ShowingLineCount = generalSetting.ShowingLineCount;
            _parent.ViewModel.ShowingLineNumber = generalSetting.ShowingNumberOfLines;
            _parent.ViewModel.ShowingWordCount = generalSetting.ShowingWordCount;
            _parent.ViewModel.ShowingNewLine = generalSetting.ShowingNewLine;
            _parent.ViewModel.ShowingTab = generalSetting.ShowingTab;
            _parent.ViewModel.ShowingSpace = generalSetting.ShowingSpace;
            _parent.ViewModel.FontColor = CommonLibrary.ConversionUtility.ConversionColorCodeToColor(generalSetting.FontColor);
            _parent.ViewModel.WallPaperColor = CommonLibrary.ConversionUtility.ConversionColorCodeToColor(generalSetting.WallPaperColor);
            _parent.ViewModel.MainFont = CommonLibrary.ConversionUtility.GetFontInfoFromFontName(generalSetting.MainFontName);
            _parent.ViewModel.SubFont = CommonLibrary.ConversionUtility.GetFontInfoFromFontName(generalSetting.SubFontName);
            _parent.ViewModel.FontSize = generalSetting.FontSize.ToString();
        }
    }
}
