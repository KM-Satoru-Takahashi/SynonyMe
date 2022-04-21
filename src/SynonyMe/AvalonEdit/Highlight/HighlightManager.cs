using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Media;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Rendering;
using SynonyMe.CommonLibrary.Log;


namespace SynonyMe.AvalonEdit.Highlight
{
    /// <summary>AvalonEditのテキストハイライトを管理するクラス</summary>
    /// todo:singleton
    internal class HighlightManager
    {
        private const string CLASS_NAME = "HighlightManager";

        #region Highlight Define

        /// <summary>
        /// テキストハイライト関連の情報を保持する内部クラス
        /// </summary>
        /// <remarks>ハイライトまわりはHighlightManagerに担わせるので、この内部クラスは公開しない</remarks>
        private class TextHighlightInfo
        {
            #region field

            /// <summary>対象テキスト文字色</summary>
            internal XshdBrush ForeGround { get; private set; }

            /// <summary>対象テキスト背景色</summary>
            internal XshdBrush BackGrouond { get; private set; }

            /// <summary>ハイライト対象語句</summary>
            internal string TargetWord { get; private set; }

            #endregion

            internal TextHighlightInfo(Color foreGround, Color backGround, string target)
            {
                ForeGround = new XshdBrush(foreGround);
                BackGrouond = new XshdBrush(backGround);
                TargetWord = target;
            }
        }

        #endregion

        private List<TextHighlightInfo> _infos = new List<TextHighlightInfo>();

        private string _xshdFilePath = null;

        private Color[] _synonymSearchBackGroundColors = CommonLibrary.Define.BACKGROUND_COLORS_DEFAULT;

        private Color _synonymSearchFontUserColor = new Color();

        private CommonLibrary.FontColorKind _synonymSearchFontColorKind = CommonLibrary.FontColorKind.Auto;

        private Color _searchBackGroundColor = CommonLibrary.Define.BACKGROUND_COLORS_DEFAULT[0];

        private Color _searchResultFontColor = new Color();

        private CommonLibrary.FontColorKind searchResultFontColorKind = CommonLibrary.FontColorKind.Auto;

        /// <summary>背景色一覧のIndexで、次に使用すべきIndexを保持します</summary>
        /// <remarks>必ずプロパティ側からGetすること</remarks>
        private int _backgroundColorIndex = 0;

        /// <summary>MainWindowのAvalonEditの背景色</summary>
        private Brush _avalonEditBackground = null;

        /// <summary>自動指定の背景色に関して、Index値を順に提供します</summary>
        private int GetBackgroundColorIndex
        {
            get
            {
                if (_synonymSearchBackGroundColors.Count() - 1 < _backgroundColorIndex)
                {
                    // 上限までいったので、リセットする
                    Logger.Info(CLASS_NAME, "GetBackGroundColorIndex", $"index is [{_backgroundColorIndex}], reset to 0");
                    _backgroundColorIndex = 0;
                }

                int i = _backgroundColorIndex;
                System.Threading.Interlocked.Increment(ref _backgroundColorIndex);
                return i;
            }
        }

        /// <summary>検索・類語検索設定変更時に発火するイベントハンドラ</summary>
        private event EventHandler<Model.Manager.Events.SettingChangedEventArgs> SearchAndSynonymSettingChangedEvent
        {
            add
            {
                Model.Manager.SettingManager.GetSettingManager.SearchAndSynonymSettingChangedEvent += value;
            }
            remove
            {
                Model.Manager.SettingManager.GetSettingManager.SearchAndSynonymSettingChangedEvent -= value;
            }
        }


        /// <summary>TextHighlightInfoが正常かどうか調べます
        /// <returns>true:正常、false:異常</returns>
        private bool IsInfoCorrect(TextHighlightInfo info)
        {
            if (info == null ||
                info.ForeGround == null ||
                info.BackGrouond == null ||
                string.IsNullOrEmpty(info.TargetWord))
            {
                Logger.Error(CLASS_NAME, "IsInfoCorrect", "info is incorrrect!");
                return false;
            }

            return true;
        }

        /// <summary>コンストラクタ</summary>
        /// <param name="wallPaper">AvalonEditの背景色（壁紙色）</param>
        internal HighlightManager(Brush wallPaper)
        {
            _avalonEditBackground = wallPaper;
            _xshdFilePath = GetXshdFilePath();
            ApplySetting(Model.Manager.SettingManager.GetSettingManager.GetSetting(typeof(Settings.SearchAndSynonymSetting)) as Settings.SearchAndSynonymSetting);

            SearchAndSynonymSettingChangedEvent -= UpdateSearchAndSynonymSettingEvent;
            SearchAndSynonymSettingChangedEvent += UpdateSearchAndSynonymSettingEvent;
        }

        private void UpdateSearchAndSynonymSettingEvent(object sender, Model.Manager.Events.SettingChangedEventArgs args)
        {
            if (args == null)
            {
                Logger.Error(CLASS_NAME, "UpdateSearchAndSynonymSettingEvent", $"args is null! sender:[{(sender == null ? "sender is null!" : $"{sender.ToString()}")}");
                return;
            }

            Settings.SearchAndSynonymSetting searchAndSynonymSetting = args.GetTargetSetting(typeof(Settings.SearchAndSynonymSetting)) as Settings.SearchAndSynonymSetting;
            if (searchAndSynonymSetting == null)
            {
                Logger.Error(CLASS_NAME, "UpdateSearchAndSynonymSettingEvent", $"searchAndSynonymSetting is null! sender:[{(sender == null ? "sender is null!" : $"{sender.ToString()}")}");
                return;
            }

            ApplySetting(searchAndSynonymSetting);
        }


        private void ApplySetting(Settings.SearchAndSynonymSetting setting)
        {
            if (setting == null)
            {
                Logger.Error(CLASS_NAME, "ApplySetting", "setting is null!");
                return;
            }

            _synonymSearchFontColorKind = setting.SynonymSearchFontColorKind;
            if (_synonymSearchFontColorKind == CommonLibrary.FontColorKind.UserSetting)
            {
                //todo:log
                _synonymSearchFontUserColor = CommonLibrary.ConversionUtility.ConversionColorCodeToColor(setting.SynonymSearchFontColor);
            }

            _synonymSearchBackGroundColors = new Color[]
            {
                CommonLibrary.ConversionUtility.ConversionColorCodeToColor(setting.SynonymSearchResultColor1),
                CommonLibrary.ConversionUtility.ConversionColorCodeToColor(setting.SynonymSearchResultColor2),
                CommonLibrary.ConversionUtility.ConversionColorCodeToColor(setting.SynonymSearchResultColor3),
                CommonLibrary.ConversionUtility.ConversionColorCodeToColor(setting.SynonymSearchResultColor4),
                CommonLibrary.ConversionUtility.ConversionColorCodeToColor(setting.SynonymSearchResultColor5),
                CommonLibrary.ConversionUtility.ConversionColorCodeToColor(setting.SynonymSearchResultColor6),
                CommonLibrary.ConversionUtility.ConversionColorCodeToColor(setting.SynonymSearchResultColor7),
                CommonLibrary.ConversionUtility.ConversionColorCodeToColor(setting.SynonymSearchResultColor8),
                CommonLibrary.ConversionUtility.ConversionColorCodeToColor(setting.SynonymSearchResultColor9),
                CommonLibrary.ConversionUtility.ConversionColorCodeToColor(setting.SynonymSearchResultColor10)
            };

            searchResultFontColorKind = setting.SearchResultFontColorKind;

            switch (searchResultFontColorKind)
            {
                case CommonLibrary.FontColorKind.Auto:
                    _searchResultFontColor = GetAutoFontColor();
                    break;

                case CommonLibrary.FontColorKind.Black:
                    _searchResultFontColor = Colors.Black;
                    break;

                case CommonLibrary.FontColorKind.White:
                    _searchResultFontColor = Colors.White;
                    break;

                case CommonLibrary.FontColorKind.UserSetting:
                    _searchResultFontColor = CommonLibrary.ConversionUtility.ConversionColorCodeToColor(setting.SearchResultFontColor);
                    break;

                default:
                    Logger.Error(CLASS_NAME, "ApplySetting", $"searchResultFontColorKind is invalid! value:[{searchResultFontColorKind}]");
                    _searchResultFontColor = GetAutoFontColor();
                    break;
            }

            _searchBackGroundColor = CommonLibrary.ConversionUtility.ConversionColorCodeToColor(setting.SearchResultBackGroundColor);

        }


        /// <summary>類語検索実施時、該当する類語に文字色と背景色を適用それぞれします</summary>
        /// <param name="targetWords"></param>
        /// <returns></returns>
        private bool CreateSynonymSearchHighlightInfos(string[] targetWords)
        {
            Logger.Info(CLASS_NAME, "CreateHighlightInfos", "start");

            if (targetWords == null || targetWords.Any() == false)
            {
                Logger.Fatal(CLASS_NAME, "CreateHighlightInfos", "targetWords are null or empty.");
                return false;
            }

            foreach (string target in targetWords)
            {
                Color backGround = _synonymSearchBackGroundColors[GetBackgroundColorIndex];

                if (string.IsNullOrEmpty(target))
                {
                    Logger.Error(CLASS_NAME, "CreateHighlightInfos", $"target is null! backGroundColor is [{backGround}]");
                    continue;
                }
                _infos.Add(new TextHighlightInfo(GetSynonymSearchFontColor(backGround), backGround, target));//todo:検索と類語検索で_fontColorとbackGroundを分ける
            }

            return true;
        }

        /// <summary>類語検索結果の文字色を取得します</summary>
        /// <param name="backGround">対象類語の背景色</param>
        /// <returns>当該類語における文字色</returns>
        private Color GetSynonymSearchFontColor(Color backGround)
        {
            switch (_synonymSearchFontColorKind)
            {
                case CommonLibrary.FontColorKind.Auto:
                    return GetAutoFontColor(backGround.R, backGround.G, backGround.B);
                case CommonLibrary.FontColorKind.Black:
                    return Colors.Black;
                case CommonLibrary.FontColorKind.White:
                    return Colors.White;
                case CommonLibrary.FontColorKind.UserSetting:
                    return _synonymSearchFontUserColor;
                default:
                    Logger.Error(CLASS_NAME, "GetSynonymSearchFontColor", $"_synonymSearchFontUserColor is incorrect! value:[{_synonymSearchFontColorKind}]");
                    return GetAutoFontColor();
            }
        }


        /// <summary>検索実施時、該当する検索結果に文字色と背景色を適用します</summary>
        /// <param name="targetWord"></param>
        /// <returns></returns>
        private bool CreateSearchHighlightInfo(string targetWord)
        {
            if (string.IsNullOrEmpty(targetWord))
            {
                Logger.Error(CLASS_NAME, "CreateSearchHighlightInfo", "targetWord is null or empty!");
                return false;
            }

            _infos.Add(new TextHighlightInfo(_searchResultFontColor, _searchBackGroundColor, targetWord));

            return true;
        }

        //todo:オーバーロード、Utilityクラスでの返還
        private Color GetAutoFontColor(byte R, byte G, byte B)
        {
            // RGBをグレースケール化する。255は白、0は黒なので、127を境界にする
            double gray = R * 0.3 + G * 0.59 + B * 0.11;
            byte byteGray = (byte)Math.Round(gray);
            Logger.Info(CLASS_NAME, "GetAutoFontColor", $"byteGray:[{byteGray}]");
            if (byteGray < 127)
            {
                // 127以下→背景色は黒に近いので、文字色は白
                return Colors.White;
            }
            else
            {
                return Colors.Black;
            }
        }

        /// <summary>AvalonEditの背景色から、文字色を取得します</summary>
        /// <returns>背景色が白寄りの場合は黒、背景色が黒寄りの場合は白</returns>
        /// <remarks>todo:設定ファイル値を参照させる、この中でカラーコード変換しない。カラーコードの変換はSettingWindowへ移譲するため</remarks>
        private Color GetAutoFontColor()
        {
            if (_avalonEditBackground == null)
            {
                Logger.Fatal(CLASS_NAME, "GetForeGroundColor", "_avalonEditBackGround is null!");
                // 異常時はとりあえず黒色の文字とする
                return Colors.Black;
            }

            // 今の背景色を[#AARRGGBB]文字列形式で取得する
            string backGroundColorCode = _avalonEditBackground.ToString();//todo:ConversionUtility
            if (string.IsNullOrEmpty(backGroundColorCode) ||
               backGroundColorCode.Length != "#AARRGGBB".Length)
            {
                string back = string.IsNullOrEmpty(backGroundColorCode) ? "" : backGroundColorCode;
                Logger.Error(CLASS_NAME, "GetForeGroundColor", $"backGroundColorCode is incorrect! value[{back}]");
                return Colors.Black;
            }

            string stringRValue = backGroundColorCode.Substring(3, 2);
            string stringGValue = backGroundColorCode.Substring(5, 2);
            string stringBValue = backGroundColorCode.Substring(7, 2);

            byte byteRValue, byteGValue, byteBValue;
            try //todo:CommonLibのUtilityで変換
            {
                byteRValue = Convert.ToByte(stringRValue, 16);
                byteGValue = Convert.ToByte(stringGValue, 16);
                byteBValue = Convert.ToByte(stringBValue, 16);
            }
            catch (Exception e)
            {
                Logger.Fatal(CLASS_NAME, "GetForeGroundColor", $"ErrorMessage:[{e.ToString()}], R:[{stringRValue}], G:[{stringGValue}], B:[{stringBValue}]");
                return Colors.Black;
            }

            return GetAutoFontColor(byteRValue, byteGValue, byteBValue);
        }

        /// <summary>
        /// ハイライト用のXshdファイルを更新します
        /// </summary>
        /// <returns>true:正常、false:異常</returns>
        internal bool UpdateXshdFile(string[] targetWords, CommonLibrary.ApplyHighlightKind kind)
        {
            Logger.Info(CLASS_NAME, "UpdateXshdFile", "start");

            // 既存の保持情報をクリアしないと、延々と残り続けてしまう
            if (ResetHighlightInfo() == false)
            {
                Logger.Fatal(CLASS_NAME, "UpdateXshdFile", "ResetHighlightInfo is incorrect!");
                return false;
            }

            switch (kind)
            {
                case CommonLibrary.ApplyHighlightKind.SynonymSearch:
                    if (CreateSynonymSearchHighlightInfos(targetWords) == false)
                    {
                        Logger.Fatal(CLASS_NAME, "UpdateXshdFile", "CreateHighlightInfos is incorrect!");
                        return false;
                    }
                    break;

                case CommonLibrary.ApplyHighlightKind.Search:
                    if (CreateSearchHighlightInfo(targetWords[0]) == false)
                    {
                        Logger.Error(CLASS_NAME, "UpdateXshdFile", $"CreateSearchHighlightInfo return false! target:[{targetWords[0]}]");
                        return false;
                    }
                    break;

                default:
                    Logger.Error(CLASS_NAME, "UpdateXshdFile", $"ApplyHighlightKing is invalid! value:[{kind}]");
                    break;
            }

            if (DeleteXshdFile() == false)
            {
                Logger.Fatal(CLASS_NAME, "UpdateXshdFile", "DeleteXshdFile is incorrect!");
                return false;
            }

            if (CreateXshdFile() == false)
            {
                Logger.Fatal(CLASS_NAME, "UpdateXshdFile", "CreateXshdFile is incorrect!");
                return false;
            }

            return SetXshdFile();
        }

        /// <summary>
        /// ハイライト設定をリセット(破棄)します
        /// </summary>
        /// <returns>true:成功, false:失敗</returns>
        internal bool ResetHighlightInfo()
        {
            Logger.Info(CLASS_NAME, "ResetHighlightInfo", "start");

            ResetBackGroundColorIndex();

            if (_infos == null)
            {
                // 空Listで初期化しているので、nullは異常と判断する
                Logger.Fatal(CLASS_NAME, "ResetHighlightInfo", "_infos is null!");
                _infos = new List<TextHighlightInfo>();
                // ただし、この後は正常系で処理を行える可能性があるため、falseをreturnすることは現状しない。
                // 問題があるようなら、将来ここでfalseをreturnすること
                return true;
            }

            _infos.Clear();
            return true;
        }

        /// <summary>
        /// ハイライト用の背景色インデックスをリセットします
        /// </summary>
        private void ResetBackGroundColorIndex()
        {
            Logger.Info(CLASS_NAME, "ResetBackGroundColorIndex", "start");
            _backgroundColorIndex = 0;
        }

        /// <summary>
        /// 保持しているInfoに従い、Xshdファイルを生成します
        /// </summary>
        /// <returns>true:成功, false:失敗</returns>
        private bool CreateXshdFile()
        {
            Logger.Info(CLASS_NAME, "CreateXshdFile", "start");

            XshdSyntaxDefinition def = new XshdSyntaxDefinition();
            def.Name = "TXT";
            def.Extensions.Add(".txt"); // .txtファイルのみ対象としているので、将来拡張するならここをいじる必要がある

            // keywordは勝手に正規表現で前後に改行コードが含まれてしまうため、見出し文字列等以外には適さない
            // ★設定で回避できる？　要調査、現状は動くことを優先して下記設定とする
            // そのため、日本語文章を対象としていることから、類語・検索語はXshdRuleSetに登録する
            XshdRuleSet xshdRuleSet = new XshdRuleSet();

            int i = 0;
            foreach (TextHighlightInfo info in _infos)
            {
                if (IsInfoCorrect(info) == false)
                {
                    Logger.Error(CLASS_NAME, "CreateXshdFile", $"info is null or incorrect! index:[{i}]");
                    continue;
                }

                XshdColor color = new XshdColor
                {
                    // Name = "keywordColor", // 別に名前は何でもいい
                    Foreground = info.ForeGround,
                    Background = info.BackGrouond,
                    // 検索結果表示を太字にする
                    //todo:設定で持たせるべきかもしれない
                    FontWeight = System.Windows.FontWeights.Bold,
                    //FontStyle = System.Windows.FontStyles.Italic これは斜体になる
                };

                string colorName = "keyword";

                // 文字毎に異なる背景色を設定したいため、ここでColorおよびColorRefのNameを紐付ける必要がある
                // 大量にあることを想定し、StringBuilderで結合する
                StringBuilder sb = new StringBuilder(colorName);
                sb.Append(i.ToString());

                color.Name = sb.ToString();
                XshdReference<XshdColor> colorRef = new XshdReference<XshdColor>(null, color.Name);

                string target = info.TargetWord;
                if (string.IsNullOrEmpty(target))
                {
                    Logger.Error(CLASS_NAME, "CreateXshdFile", $"target is null! target:[{target}]");
                    continue;
                }

                XshdRule rule = new XshdRule
                {
                    ColorReference = colorRef,
                    Regex = target // 正規表現で持たせる必要があるが、文字単位なのでそのまま渡して問題ない                    
                };

                xshdRuleSet.Elements.Add(rule);

                // 追加したいモノは追加した
                def.Elements.Add(color);
                System.Threading.Interlocked.Increment(ref i);
            }

            def.Elements.Add(xshdRuleSet);

            return WriteXshdFile(def);
        }

        /// <summary>
        /// Xshdファイルを書き込みます
        /// </summary>
        /// <param name="def">書き込み対象のDefinitionインスタンス</param>
        /// <returns>true:成功, false:失敗</returns>
        private bool WriteXshdFile(XshdSyntaxDefinition def)
        {
            Logger.Info(CLASS_NAME, "WriteXshdFile", "start");

            if (def == null)
            {
                Logger.Error(CLASS_NAME, "WriteXshdFile", "def is null!");
                return false;
            }

            using (XmlTextWriter writer = new XmlTextWriter(_xshdFilePath, System.Text.Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;

                SaveXshdVisitor visitor = new SaveXshdVisitor(writer);

                // 実装側でどう処理しているか調査終えるまで、念のためtry-catchしておく
                try
                {
                    visitor.WriteDefinition(def);
                }
                catch (Exception e)
                {
                    Logger.Fatal(CLASS_NAME, "WriteXshdFile", e.ToString());
                    return false;
                }
                finally
                {
                    writer.Close();
                }
            }

            return true;
        }

        /// <summary>
        /// Xshdファイルを画面のAvalonEditに適用させ、ハイライトを有効にします
        /// </summary>
        /// <returns>true:成功, false:失敗</returns>
        private bool SetXshdFile()
        {
            Logger.Info(CLASS_NAME, "SetXshdFile", "start");

            View.MainWindow mw = Model.Manager.WindowManager.GetMainWindow();

            ICSharpCode.AvalonEdit.TextEditor target = mw.TextEditor;
            if (target == null)
            {
                Logger.Fatal(CLASS_NAME, "SetXshdFile", "GetTextEditor TextEditor is null");
                return false;
            }

            try
            {
                using (XmlReader reader = new XmlTextReader(_xshdFilePath))
                {
                    var definition = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                    target.SyntaxHighlighting = definition;
                    reader.Close();
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(CLASS_NAME, "SetXshdFile", e.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// 既存のXshdファイルを削除します
        /// </summary>
        /// <returns>true:成功, false:失敗</returns>
        private bool DeleteXshdFile()
        {
            Logger.Info(CLASS_NAME, "DeleteXshdFile", "start");

            // FileInfoはコンストラクタ生成時にのみ正規化するが、Fileは都度正規化する
            // 削除処理は多くの回数呼ばれることが想定されるため、パフォーマンスを考慮してFileInfoを使用する
            string xshdFilePath = GetXshdFilePath();
            FileInfo fi = new FileInfo(xshdFilePath);
            if (fi.Exists == false)
            {
                Logger.Fatal(CLASS_NAME, "DeleteXshdFile", $"File is not exist! filePath:[{xshdFilePath}]");
                return false;
            }

            // 読み取り専用属性は付与させない想定だが、念のため考慮する
            if ((fi.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            {
                Logger.Info(CLASS_NAME, "DeleteXshdFile", "FileAttributes is ReadOnly, change FileAttributes to Normal");
                fi.Attributes = FileAttributes.Normal;
            }

            try
            {
                fi.Delete();
            }
            catch (Exception e)
            {
                Logger.Fatal(CLASS_NAME, "DeleteXshdFile", e.ToString());
                return false;
            }

            return true;
        }


        /// <summary>
        /// 処理対象のXshdファイルパスを取得します
        /// </summary>
        /// <returns>Xshd設定ファイルへの絶対パス</returns>
        private string GetXshdFilePath()
        {
            string filePath = CommonLibrary.SystemUtility.GetSynonymeExeFilePath();
            if (string.IsNullOrEmpty(filePath))
            {
                Logger.Fatal(CLASS_NAME, "GetXshdFilePath", "filePath is null or empty!");
                return null;
            }

            // ファイル名情報は不要なので削除する
            filePath = filePath.Replace(CommonLibrary.Define.SYNONYME_EXENAME, "");

            // Xshdへのファイルパスを構築する。[filePath]直後の[\]はファイル名ではなく、Replaceで削除されていないので、直に連結してOK
            filePath = filePath + @"work\" + CommonLibrary.Define.XSHD_FILENAME;
            Logger.Info(CLASS_NAME, "GetXshdFilePath", $"XshdFilePath:[{filePath}]");

            return filePath;
        }
    }
}
