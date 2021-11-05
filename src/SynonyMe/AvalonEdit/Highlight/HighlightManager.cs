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
    /// <summary>
    /// AvalonEditのテキストハイライトを管理するクラス
    /// </summary>
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

        /// <summary>背景色候補となる色定義</summary>
        /// <remarks>将来はユーザが設定ツールから、デフォルトか自分で設定した色リストかを選択可能とすることも想定するが、
        /// 現状は判別しやすい下記の色定義を使用する</remarks>
        private readonly Color[] BACKGROUND_COLORS_DEFAULT = new Color[]
        {
            Colors.HotPink,
            Colors.Cyan,
            Colors.Yellow,
            Colors.Lime,
            Colors.Violet,
            Colors.Red,
            Colors.Blue,
            Colors.Chocolate,
            Colors.Green,
            Colors.DarkViolet,
            Colors.Gray,
            // この下は20個に満たないから強引に足した色なので、↑のと組み合わせがいいかは正直微妙
            Colors.DarkSalmon,
            Colors.Goldenrod,
            Colors.Pink,
            Colors.MediumOrchid,
            Colors.MidnightBlue,
            Colors.SteelBlue,
            Colors.DarkOrange,
            Colors.LimeGreen,
            Colors.ForestGreen
        };

        /// <summary>背景色一覧のIndexで、次に使用すべきIndexを保持します</summary>
        /// <remarks>必ずプロパティ側からGetすること</remarks>
        private int _backgroundColorIndex = 0;

        /// <summary>MainWindowのAvalonEditの背景色</summary>
        private Brush _avalonEditBackground = null;

        /// <summary>自動指定の背景色に関して、Index値を順にを提供します</summary>
        private int GetBackgroundColorIndex
        {
            get
            {
                if (BACKGROUND_COLORS_DEFAULT.Count() - 1 < _backgroundColorIndex)
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

        /// <summary>
        /// TextHighlightInfoが正常かどうか調べます
        /// </summary>
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


        internal HighlightManager(Brush backGround)
        {
            _avalonEditBackground = backGround;
            _xshdFilePath = GetXshdFilePath();
        }

        private bool CreateHighlightInfos(string[] targetWords)
        {
            Logger.Info(CLASS_NAME, "CreateHighlightInfos", "start");

            if (targetWords == null || targetWords.Any() == false)
            {
                Logger.Fatal(CLASS_NAME, "CreateHighlightInfos", "targetWords are null or empty.");
                return false;
            }

            Color foreGround = GetForeGroundColor();

            foreach (string target in targetWords)
            {
                Color backGround = BACKGROUND_COLORS_DEFAULT[GetBackgroundColorIndex];

                if (string.IsNullOrEmpty(target))
                {
                    Logger.Error(CLASS_NAME, "CreateHighlightInfos", $"target is null! backGroundColor is [{backGround.ToString()}]");
                    continue;
                }
                _infos.Add(new TextHighlightInfo(foreGround, backGround, target));
            }

            return true;
        }

        /// <summary>AvalonEditの背景色から、文字色を取得します</summary>
        /// <returns>背景色が白寄りの場合は黒、背景色が黒寄りの場合は白</returns>
        private Color GetForeGroundColor()
        {
            if (_avalonEditBackground == null)
            {
                Logger.Fatal(CLASS_NAME, "GetForeGroundColor", "_avalonEditBackGround is null!");
                // 異常時はとりあえず黒色の文字とする
                return Colors.Black;
            }

            // 今の背景色を[#AARRGGBB]文字列形式で取得する
            string backGroundColorCode = _avalonEditBackground.ToString();
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
            try
            {
                byteRValue = Convert.ToByte(stringRValue, 16);
                byteGValue = Convert.ToByte(stringGValue, 16);
                byteBValue = Convert.ToByte(stringBValue, 16);
            }
            catch (Exception e)
            {
                Logger.Fatal(CLASS_NAME, "GetForeGroundColor", $"ErrorMessage:[{e.Message}], R:[{stringRValue}], G:[{stringGValue}], B:[{stringBValue}]");
                return Colors.Black;
            }

            // RGBをグレースケール化する。255は白、0は黒なので、127を境界にする
            double gray = byteRValue * 0.3 + byteGValue * 0.59 + byteBValue * 0.11;
            byte byteGray = (byte)Math.Round(gray);
            Logger.Info(CLASS_NAME, "GetForeGroundColor", $"byteGray:[{byteGray}]");
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

        /// <summary>
        /// ハイライト用のXshdファイルを更新します
        /// </summary>
        /// <returns>true:正常、false:異常</returns>
        internal bool UpdateXshdFile(string[] targetWords)
        {
            Logger.Info(CLASS_NAME, "UpdateXshdFile", "start");

            // 既存の保持情報をクリアしないと、延々と残り続けてしまう
            if (ResetHighlightInfo() == false)
            {
                Logger.Fatal(CLASS_NAME, "UpdateXshdFile", "ResetHighlightInfo is incorrect!");
                return false;
            }

            if (CreateHighlightInfos(targetWords) == false)
            {
                Logger.Fatal(CLASS_NAME, "UpdateXshdFile", "CreateHighlightInfos is incorrect!");
                return false;
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
        private bool ResetHighlightInfo()
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
                    Background = info.BackGrouond
                };

                string colorName = "keyword";

                // 文字毎に異なる背景色を設定したいため、ここでColorおよびColorRefのNameを紐付ける必要がある
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
                    Logger.Fatal(CLASS_NAME, "WriteXshdFile", e.Message);
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

            MainWindow mw = Model.Manager.WindowManager.GetMainWindow();

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
                Logger.Fatal(CLASS_NAME, "SetXshdFile", e.Message);
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
                Logger.Fatal(CLASS_NAME, "DeleteXshdFile", e.Message);
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
