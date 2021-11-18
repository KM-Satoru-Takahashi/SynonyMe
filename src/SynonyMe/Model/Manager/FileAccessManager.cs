using SynonyMe.CommonLibrary.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynonyMe.Model.Manager
{
    /// <summary>ファイル開閉や読み書きを担うクラス</summary>
    internal class FileAccessManager
    {
        private const string CLASS_NAME = "FileAccessManager";

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

    }
}
