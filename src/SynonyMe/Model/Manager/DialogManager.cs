using Microsoft.Win32; // Forms名前空間のSaveFileDialogとは別物なので要注意
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SynonyMe.CommonLibrary.Log;

namespace SynonyMe.Model.Manager
{
    /// <summary>各種ダイアログの表示を管理します</summary>
    internal static class DialogManager
    {
        private const string DIALOG_FILTER_TXTandALL = "テキスト ファイル(.txt)|*.txt|All Files (*.*)|*.*";

        private const string CLASS_NAME = "DialogManager";

        internal static bool OpenFileSaveDialog(out SaveFileDialog dialog)
        {
            dialog = new SaveFileDialog();
            dialog.Filter = DIALOG_FILTER_TXTandALL;
            dialog.FilterIndex = 1;

            bool? result = dialog.ShowDialog();
            if (result != null && result == true)
            {
                Logger.Info(CLASS_NAME, "OpenFileSaveDialog", "file save.");
                return true;
            }
            else
            {
                Logger.Info(CLASS_NAME, "OpenFileSaveDialog", $"save canceled. result:[{result}]");
                return false;
            }
        }

    }
}
