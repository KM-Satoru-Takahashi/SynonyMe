using Microsoft.Win32; // Forms名前空間のSaveFileDialogとは別物なので要注意
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SynonyMe.CommonLibrary.Log;
using System.IO;
using System.Windows.Forms;

namespace SynonyMe.Model.Manager
{
    /// <summary>各種ダイアログの表示を管理します</summary>
    /// todo:FileAccessManagerに必要な処理を委譲すること
    internal class DialogManager
    {
        private const string DIALOG_FILTER_TXTandALL = "テキスト ファイル(.txt)|*.txt|All Files (*.*)|*.*";

        private const string CLASS_NAME = "DialogManager";

        private const string Caption_OkCancelMessageBox = "確認";

        private static DialogManager _dialogManager = new DialogManager();

        /// <summary>ダイアログ表示管理クラス</summary>
        internal static DialogManager GetDialogManager
        {
            get
            {
                return _dialogManager;
            }
        }

        /// <summary>シングルトン担保</summary>
        private DialogManager()
        { }

        /// <summary>OK Cancelダイアログを表示して確認をとります</summary>
        /// <param name="displayMessage">表示メッセージ</param>
        /// <param name="result">押下結果</param>
        /// <returns>true:正常, false:異常</returns>
        internal bool OpenOkCancelDialog(string displayMessage, out DialogResult result)
        {
            result = DialogResult.Cancel;
            if (string.IsNullOrEmpty(displayMessage))
            {
                Logger.Error(CLASS_NAME, "OpenOkCancelDialog", "displayMessage is null or empty!");
                return false;
            }

            try
            {
                result = MessageBox.Show(displayMessage, Caption_OkCancelMessageBox, MessageBoxButtons.OKCancel);
            }
            catch (Exception e)
            {
                Logger.Fatal(CLASS_NAME, "OpenOkCancelDialog", $"result:[{result}], message:{e.Message}");
                return false;
            }

            return true;
        }

        /// <summary>名前をつけて保存ダイアログを開く</summary>
        /// <param name="saveFilePath">保存したファイルパス</param>
        /// <returns>true:成功, false:失敗</returns>
        internal bool OpenSaveAsDialog(out string saveFilePath)
        {
            saveFilePath = string.Empty;

            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog
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

            saveFilePath = dialog.FileName;

            return true;
        }

        /// <summary>ファイルを開くダイアログを表示します</summary>
        /// <returns></returns>
        internal bool OpenFileOpenDialog(out string openFilePath)
        {
            openFilePath = string.Empty;

            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = DIALOG_FILTER_TXTandALL,
                FilterIndex = 1,
                InitialDirectory = @"C:\",
                Title = "開く",
                FileName = ""
            };

            bool? dialogResult = dialog.ShowDialog();
            if (dialogResult == null || dialogResult != true)
            {
                // 失敗時処理
                Logger.Info(CLASS_NAME, "OpenFileOpenDialog", $"open canceled. result:[{dialogResult}]");
                return false;
            }

            openFilePath = dialog.FileName;

            return true;
        }
    }
}
