using SynonyMe.CommonLibrary.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using static SynonyMe.CommonLibrary.SystemUtility;

namespace SynonyMe.CommonLibrary
{
    /// <summary>各種変換処理を提供します</summary>
    internal static class ConversionUtility
    {
        private const string CLASS_NAME = "ConversionUtility";

        /// <summary>文字列[#AARRGGBB]からColor構造体を作成します</summary>
        /// <param name="code">#AARRGGBB形式である9桁の文字列</param>
        /// <returns>Color構造体(失敗時はデフォルト値)</returns>
        internal static Color ConversionColorCodeToColor(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                Logger.Error(CLASS_NAME, "ConversionColorCodeToColor", "code is null or empty!");
                return new Color();
            }

            if (code.Length != "#AARRGGBB".Length)
            {
                Logger.Error(CLASS_NAME, "ConversionColorCodeToColor", $"code is invalid! code:[{code}]");
                return new Color();
            }

            string stringAValue = code.Substring(1, 2);
            string stringRValue = code.Substring(3, 2);
            string stringGValue = code.Substring(5, 2);
            string stringBValue = code.Substring(7, 2);

            byte byteAValue, byteRValue, byteGValue, byteBValue;

            if (byte.TryParse(stringAValue, System.Globalization.NumberStyles.HexNumber, null, out byteAValue) == false)
            {
                Logger.Error(CLASS_NAME, "ConversionColorCodeToColor", $"AValue is invalid.[{stringAValue}]");
                byteAValue = 255;
            }
            if (byte.TryParse(stringRValue, System.Globalization.NumberStyles.HexNumber, null, out byteRValue) == false)
            {
                Logger.Error(CLASS_NAME, "ConversionColorCodeToColor", $"RValue is invalid.[{stringRValue}]");
                byteRValue = 255;
            }
            if (byte.TryParse(stringGValue, System.Globalization.NumberStyles.HexNumber, null, out byteGValue) == false)
            {
                Logger.Error(CLASS_NAME, "ConversionColorCodeToColor", $"GValue is invalid.[{stringGValue}]");
                byteGValue = 255;
            }
            if (byte.TryParse(stringBValue, System.Globalization.NumberStyles.HexNumber, null, out byteBValue) == false)
            {
                Logger.Error(CLASS_NAME, "ConversionColorCodeToColor", $"GValue is invalid.[{stringBValue}]");
                byteBValue = 255;
            }

            Color color = new Color()
            {
                A = byteAValue,
                R = byteRValue,
                G = byteGValue,
                B = byteBValue
            };
            return color;
        }

        /// <summary>フォント名称からFontInfoを取得します</summary>
        /// <param name="fontName">対象のフォント名</param>
        /// <returns>true:成功, false:失敗</returns>
        /// <remarks>フォント名設定コンボボックスに表示されているフォント名称と合致するかを判定するので、引数の日本語/英語は考慮しなくて良い</remarks>
        internal static FontInfo GetFontInfoFromFontName(string fontName)
        {
            if (string.IsNullOrEmpty(fontName))
            {
                Logger.Error(CLASS_NAME, "GetFontInfoFromName", "fontName is null or empty!");
                return null;
            }

            if (FontInfos == null || FontInfos.Any() == false)
            {
                Logger.Fatal(CLASS_NAME, "GetFontInfoFromName", "FontList is null or empty!");
                return null;
            }

            FontInfo target = null;
            foreach (FontInfo fontInfo in FontInfos) //todo:高速化, 重複することないのでParallel等を検討
            {
                if (fontInfo == null)
                {
                    // ログを出せる情報がない
                    continue;
                }

                if (fontInfo.FontName.Equals(fontName))
                {
                    target = fontInfo;
                    break;
                }
            }

            if (target == null)
            {
                Logger.Error(CLASS_NAME, "GetFontInfoFromFontName", $"target is null! fontName:[{fontName}]");
            }

            return target;
        }

    }
}
