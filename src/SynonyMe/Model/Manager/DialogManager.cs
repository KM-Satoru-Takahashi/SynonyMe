using Microsoft.Win32; // Forms名前空間のSaveFileDialogとは別物なので要注意
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SynonyMe.CommonLibrary.Log;
using System.IO;

namespace SynonyMe.Model.Manager
{
    /// <summary>各種ダイアログの表示を管理します</summary>
    internal static class DialogManager
    {
        private const string DIALOG_FILTER_TXTandALL = "テキスト ファイル(.txt)|*.txt|All Files (*.*)|*.*";

        private const string CLASS_NAME = "DialogManager";

        internal static bool OpenSaveAsDialog(string targetText, out string saveFilePath)
        {
            saveFilePath = string.Empty;

            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = DIALOG_FILTER_TXTandALL,
                FilterIndex = 1,
                InitialDirectory = @"C:\",
                Title = "名前をつけて保存",
                FileName = "新しいテキスト.txt"
            };

            bool? dialogResult = dialog.ShowDialog();
            if (dialogResult == null || dialogResult != true)
            {
                // 失敗時処理
                Logger.Info(CLASS_NAME, "OpenFileSaveDialog", $"save canceled. result:[{dialogResult}]");
                return false;
            }

            // 成功時、空文字でファイル保存しておかないとメモリ上に存在するのみになる
            bool saveResult = SaveFile(targetText, dialog.FileName);
            if (saveResult)
            {
                Logger.Info(CLASS_NAME, "OpenSaveAsDialog", $"SaveFile Success! FilePath:[{dialog.FileName}]");
                saveFilePath = dialog.FileName;
                return true;
            }
            else
            {
                Logger.Error(CLASS_NAME, "OpenSaveAsDialog", "SaveFile Failed!");
                return false;
            }
        }

        /// <summary>新規作成したファイルを空文字で保存する</summary>
        /// <param name="targetFilePath"></param>
        /// <returns></returns>
        private static bool SaveNewFile(string targetFilePath)
        {
            Logger.Info(CLASS_NAME, "SaveNewFile", $"start. targetFilePath:[{targetFilePath}]");
            return SaveFile("", targetFilePath);
        }

        /// <summary>指定されたパスとテキストでファイルを保存します</summary>
        /// <param name="targetFilePath"></param>
        /// <param name="targetText"></param>
        /// <returns></returns>
        internal static bool SaveFile(string targetText, string targetFilePath)
        {
            // targetTextが空文字はあり得るので要注意(新規作成時や空文字保存時)
            if (targetText == null || string.IsNullOrEmpty(targetFilePath))
            {
                // error log
                return false;
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
                // error log
                return false;
            }

            return true;
        }

    }
}
