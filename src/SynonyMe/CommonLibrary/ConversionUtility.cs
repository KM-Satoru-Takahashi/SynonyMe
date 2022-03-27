using SynonyMe.CommonLibrary.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SynonyMe.CommonLibrary
{
    /// <summary>各種変換処理を提供します</summary>
    internal static class ConversionUtility
    {
        private const string CLASS_NAME = "ConversionUtility";

        /// <summary>文字列[#AARRGGBB]からColor構造体を作成します</summary>
        /// <param name="code">#AARRGGBB形式である9桁の文字列</param>
        /// <returns>Color構造体(失敗時はデフォルト値)</returns>
        internal static Color ConversitonColorCodeToColor(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                Logger.Error(CLASS_NAME, "ConversitonColorCodeToColor", "code is null or empty!");
                return new Color();
            }

            if (code.Length != "#AARRGGBB".Length)
            {
                Logger.Error(CLASS_NAME, "ConversitonColorCodeToColor", $"code is invalid! code:[{code}]");
                return new Color();
            }

            string stringAValue = code.Substring(1, 2);
            string stringRValue = code.Substring(3, 2);
            string stringGValue = code.Substring(5, 2);
            string stringBValue = code.Substring(7, 2);

            byte byteAValue, byteRValue, byteGValue, byteBValue;

            if (byte.TryParse(stringAValue, System.Globalization.NumberStyles.HexNumber, null, out byteAValue) == false)
            {
                Logger.Error(CLASS_NAME, "ConversitonColorCodeToColor", $"AValue is invalid.[{stringAValue}]");
                byteAValue = 255;
            }
            if (byte.TryParse(stringRValue, System.Globalization.NumberStyles.HexNumber, null, out byteRValue) == false)
            {
                Logger.Error(CLASS_NAME, "ConversitonColorCodeToColor", $"RValue is invalid.[{stringRValue}]");
                byteRValue = 255;
            }
            if (byte.TryParse(stringGValue, System.Globalization.NumberStyles.HexNumber, null, out byteGValue) == false)
            {
                Logger.Error(CLASS_NAME, "ConversitonColorCodeToColor", $"GValue is invalid.[{stringGValue}]");
                byteGValue = 255;
            }
            if (byte.TryParse(stringBValue, System.Globalization.NumberStyles.HexNumber, null, out byteBValue) == false)
            {
                Logger.Error(CLASS_NAME, "ConversitonColorCodeToColor", $"GValue is invalid.[{stringBValue}]");
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
    }
}
