using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using SynonyMe.CommonLibrary.Log;

namespace SynonyMe.CommonLibrary
{
    /// <summary>WindowsのSystemまわりのメソッドで必要なものを実装し、提供するクラス</summary>
    public static class SystemUtility
    {
        private static string CLASS_NAME = "SystemUtility";

        internal static FontInfo[] FontInfos = GetSystemFontName();

        /// <summary>
        /// 起動中exe(SynonyMe.exe)の絶対パスを取得します
        /// </summary>
        /// <returns>取得できなかった場合はnull</returns>
        internal static string GetSynonymeExeFilePath()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            if (assembly == null)
            {
                Logger.Fatal(CLASS_NAME, "GeySynonymeExeFilePath", "assembly is null!");
                return null;
            }

            string synonymeExePath = System.Reflection.Assembly.GetExecutingAssembly().Location;

            if (string.IsNullOrEmpty(synonymeExePath))
            {
                Logger.Fatal(CLASS_NAME, "GetSynonymeExeFilePath", "synonymeExePath is null or empty!");
                return null;
            }

            return synonymeExePath;
        }

        /// <summary>フォント名称からFontInfoを取得します</summary>
        /// <param name="fontName">対象のフォント名</param>
        /// <returns>true:成功, false:失敗</returns>
        /// <remarks>フォント名設定コンボボックスに表示されているフォント名称と合致するかを判定するので、引数の日本語/英語は考慮しなくて良い</remarks>
        /// <summary>選択可能な全フォント一覧を取得します</summary>
        internal static FontInfo[] GetSystemFontName()
        {
            //todo:起動時に一度呼ぶだけで良いので、設定ファイルの読み込み箇所とかに移動させるべき
            // 日本語フォントを日本語で表示したいので、現在動いている環境の言語を取得する
            //todo:WinwowのLanguageに、このlanguageを指定する必要がある
            System.Windows.Markup.XmlLanguage language = System.Windows.Markup.XmlLanguage.GetLanguage
                (System.Threading.Thread.CurrentThread.CurrentCulture.Name);

            // 使える全言語を取得
            var fonts = Fonts.SystemFontFamilies.Select
                 (i => new FontInfo() { FontFamily = i, FontName = i.Source });

            // IEnumerableのままだと要素の更新ができないので、一旦ローカルで受け取る
            FontInfo[] fontsArr = fonts.ToArray();

            // このままだと日本語で表示してくれないので、日本語のものはこちらで取得して入れ込み、表示してやる
            foreach (var fontInfo in fontsArr)
            {
                foreach (var familyName in fontInfo.FontFamily.FamilyNames)
                {
                    if (familyName.Key == language && familyName.Value != null)
                    {
                        fontInfo.FontName = familyName.Value;
                        break;
                    }
                }
            }

            return fontsArr.ToList().ToArray();//todo:ToArrayしているので型を配列にする
        }


        public class FontInfo
        {
            public FontFamily FontFamily { get; set; }
            public string FontName { get; set; }
        }
    }
}
