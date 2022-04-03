using SynonyMe.CommonLibrary.Log;
using SynonyMe.Model.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SynonyMe.Model.MainWindow
{
    internal class ToolbarModel
    {
        private MainWindowModel _parent = null;

        private const string CLASS_NAME = "ToolbarModel";

        public ToolbarModel(MainWindowModel parent)
        {
            _parent = parent;
        }

        /// <summary>テキストファイルを新規作成します</summary>
        /// <remarks>true:正常, false:異常</remarks>
        internal bool CreateNewFile()
        {
            // 現在表示中のテキストが編集済みか否かを判定する
            if (_parent.ViewModel.IsModified)
            {
                // 保存されていなければ、Yes/Noダイアログを出して確認する
                DialogResult dialogResult = DialogResult.Cancel;
                bool result = DialogManager.GetDialogManager.OpenOkCancelDialog("現在編集中の文章を破棄しますか？", out dialogResult);
                if (result == false)
                {
                    Logger.Fatal(CLASS_NAME, "CreateNewFile", $"Dialog error! dialogResult:[{dialogResult}]");
                    return false;
                }

                if (dialogResult == DialogResult.Cancel)
                {
                    Logger.Info(CLASS_NAME, "CreateNewFile", "Canceled discard text and create new file");
                    return true;
                }
            }

            // 破棄OKか、保存済みであれば現在表示中のテキストとXshdをクリアする
            _parent.TextDocument.Text = string.Empty;
            _parent.HighlightManager.ResetHighlightInfo();

            // ファイル保存ダイアログを表示する
            string saveFilePath = null;
            if (DialogManager.GetDialogManager.OpenSaveAsDialog(out saveFilePath) == false)
            {
                Logger.Info(CLASS_NAME, "CreateNewFile", "create new file failed.");
                return false;
            }

            if (string.IsNullOrEmpty(saveFilePath))
            {
                Logger.Fatal(CLASS_NAME, "CreateNewFile", "Filename is null or empty!");
                return false;
            }

            // ファイルを保存する
            if (FileAccessor.GetFileAccessor.SaveNewFile(saveFilePath) == false)
            {
                Logger.Error(CLASS_NAME, "CreateNewFile", $"SaveNewFile failed. SaveFilePath:[{saveFilePath}]");
                return false;
            }

            // 保存したファイルパスを保持する
            _parent.DisplayTextFilePath = saveFilePath;

            // 編集済みフラグを下げる
            _parent.ViewModel.IsModified = false;

            // 名前をつけて保存フラグを下げる
            _parent.ForceSaveAs = false;

            return true;

        }
    }
}
