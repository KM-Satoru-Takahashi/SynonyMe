using SynonyMe.CommonLibrary.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace SynonyMe.Model
{
    /// <summary>ファイル開閉や読み書きを担うクラス</summary>
    /// <remarks>シングルトン</remarks>
    internal class FileAccessor
    {
        private const string CLASS_NAME = "FileAccessManager";

        private static FileAccessor _fileAccessor = new FileAccessor();
        internal static FileAccessor GetFileAccessor
        {
            get
            {
                return _fileAccessor;
            }
        }

        /// <summary>シングルトン担保</summary>
        private FileAccessor()
        { }

        /// <summary>新規作成したファイルを空文字で保存する</summary>
        /// <param name="targetFilePath"></param>
        /// <returns></returns>
        internal bool SaveNewFile(string targetFilePath)
        {
            Logger.Info(CLASS_NAME, "SaveNewFile", $"start. targetFilePath:[{targetFilePath}]");
            return SaveFile(null, targetFilePath);
        }

        /// <summary>指定されたパスとテキストでファイルを保存します</summary>
        /// <param name="targetFilePath"></param>
        /// <param name="targetText"></param>
        /// <returns></returns>
        internal bool SaveFile(string targetText, string targetFilePath)
        {
            // targetTextがnull/空文字はあり得るので要注意(新規作成時や空文字保存時)
            if (string.IsNullOrEmpty(targetFilePath))
            {
                Logger.Error(CLASS_NAME, "SaveFile", "targetFilePath is null or empty!");
                return false;
            }

            if (File.Exists(targetText))
            {
                // このままでは保存できないので一旦呼び出し元に処理を戻す
                // todo
            }

            // 将来はエンコーディングの引数を追加するが、現在はUTF-8固定とする
            Encoding enc = Encoding.UTF8;
            try
            {
                using (StreamWriter sw = new StreamWriter(targetFilePath, false, enc)) // 上書き保存するので、第二引数にfalseを指定する
                {
                    sw.WriteLine(targetText);
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(CLASS_NAME, "SaveFile", e.Message);
                return false;
            }

            return true;
        }

        /// <summary>対象の設定ファイルを保存します</summary>
        /// <param name="fileName">保存対象ファイル名（パスではない）</param>
        /// <param name="target">対象オブジェクト</param>
        /// <param name="targetType">保存対象ファイル種類</param>
        /// <returns>true:成功, false:失敗</returns>
        internal bool SaveSettingFile(string fileName, object target, Type targetType)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                // todo log
                return false;
            }

            if (target == null)
            {
                // todo log
                return false;
            }

            XmlWriterSettings settings = new XmlWriterSettings
            {
                // xml宣言の省略はさせない
                OmitXmlDeclaration = false,
                // xml出力時のインデント設定
                Encoding = System.Text.Encoding.UTF8,
                Indent = true,
                IndentChars = "  "
            };

            using (var writer = XmlWriter.Create(GetSettingFilePath(fileName), settings))
            {
                XmlSerializer serializer = new XmlSerializer(targetType);
                try
                {
                    serializer.Serialize(writer, target);
                }
                catch
                {
                    //todo:error log
                }
            }

            return false;
        }

        internal XmlDocument LoadSettingFile()
        {
            string targetFilePath = GetSettingFilePath(CommonLibrary.Define.SETTING_FILENAME_ADVANCED);
            if (string.IsNullOrEmpty(targetFilePath))
            {
                //todo error log 
                return null;
            }

            // todo:File.Existで確認させるべき

            XmlDocument xml = new XmlDocument();
            try
            {
                xml.Load(@targetFilePath);
            }
            catch
            {
                // bakファイルから読み取れないか試みる
                // todo
            }

            return xml;
        }

        /// <summary>指定された設定ファイルまでの絶対パスを取得します</summary>
        /// <returns></returns>
        private string GetSettingFilePath(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                Logger.Fatal(CLASS_NAME, "GetSettingFilePath", "fileName is null or empty!");
                return null;
            }

            string filePath = CommonLibrary.SystemUtility.GetSynonymeExeFilePath();
            if (string.IsNullOrEmpty(filePath))
            {
                Logger.Fatal(CLASS_NAME, "GetSettingFilePath", "filePath is null or empty!");
                return null;
            }

            // ファイル名情報は不要なので削除する
            filePath = filePath.Replace(CommonLibrary.Define.SYNONYME_EXENAME, "");

            // SynonyMe/Settingsへのファイルパスを構築する。[filePath]直後の[\]はファイル名ではなく、Replaceで削除されていないので、直に連結してOK
            filePath = filePath + @"Settings\" + fileName;

            // 設定ファイル読み書き側でログ出力しているので、ここではログ出ししない
            // Logger.Info(CLASS_NAME, "GetSettingFilePath", $"SettingDirectoryPath:[{filePath}]");

            return filePath;
        }

    }
}
