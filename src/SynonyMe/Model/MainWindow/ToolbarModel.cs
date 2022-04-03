using ICSharpCode.AvalonEdit;
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
            if (IsParentOrViewModelIsNull())
            {
                Logger.Error(CLASS_NAME, "CreateNewFile", "IsParentOrViewModelIsNull return true!");
                return false;
            }

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
            if (_parent.TextDocument == null)
            {
                Logger.Fatal(CLASS_NAME, "CreateNewFile", "TextDocument is null!");
                return false;
            }
            _parent.TextDocument.Text = string.Empty;

            if (_parent.HighlightManager == null)
            {
                Logger.Fatal(CLASS_NAME, "CreateNewFile", "HighlightManager is null!");
                return false;
            }
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
            if (FileAccessor.GetFileAccessor.SaveNewFile(saveFilePath))
            {
                Logger.Info(CLASS_NAME, "CreateNewFile", $"SaveNewFile successed! SaveFilePath:[{saveFilePath}]");
            }
            else
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

        /// <summary>名前をつけて保存処理を実行します</summary>
        /// <returns>true:成功, false:失敗</returns>
        internal bool SaveAs()
        {
            Logger.Info(CLASS_NAME, "SaveAs", "start");

            if (IsParentOrViewModelIsNull())
            {
                Logger.Error(CLASS_NAME, "SaveAs", "IsParentOrViewModelIsNull return true!");
                return false;
            }

            string saveFilePath = string.Empty;
            bool result = DialogManager.GetDialogManager.OpenSaveAsDialog(out saveFilePath);

            // 失敗時はログとエラーダイアログを出す
            if (result == false)
            {
                Logger.Error(CLASS_NAME, "SaveAs", $"SaveAs Failed! SaveFilePath:[{saveFilePath}]");

                // todo error dialog
                return false;
            }

            if (FileAccessor.GetFileAccessor.SaveFile(_parent.TextDocument.Text, saveFilePath))
            {
                Logger.Info(CLASS_NAME, "SaveAs", $"SaveAs successed! SaveFilePath:[{saveFilePath}]");
            }
            else
            {
                Logger.Error(CLASS_NAME, "SaveAs", $"SaveFile Failed. saveFilePath:[{saveFilePath}]");
                return false;
            }

            // 保持している、現在開いているファイル情報を更新する
            _parent.DisplayTextFilePath = saveFilePath;

            // AvalonEditの編集済みフラグをOffにする
            _parent.ViewModel.IsModified = false;

            // 名前をつけて保存フラグをOffにする
            _parent.ForceSaveAs = false;
            return true;
        }

        /// <summary>上書き保存を実行します</summary>
        /// <param name="displayText"></param>
        /// <returns></returns>
        internal bool Save()
        {
            if (IsParentOrViewModelIsNull())
            {
                Logger.Error(CLASS_NAME, "Save", "IsParentOrViewModelIsNull return true!");
                return false;
            }

            Logger.Info(CLASS_NAME, "Save",
                $"start. filePath:[{(string.IsNullOrEmpty(_parent.DisplayTextFilePath) ? "DisplayTextFile is null or empty, create new file!" : _parent.DisplayTextFilePath)}]");

            // 名前をつけて保存を実行する
            if (_parent.ForceSaveAs)
            {
                return SaveAs();
            }

            if (string.IsNullOrEmpty(_parent.DisplayTextFilePath))
            {
                Logger.Fatal(CLASS_NAME, "Save", "filePath is null or empty!");
                return false;
            }

            if (_parent.TextDocument == null)
            {
                Logger.Fatal(CLASS_NAME, "Save", "TextDocument is null!");
                return false;
            }

            if (_parent.TextDocument.Text == null) // 保存文書が空文字の場合、Emptyはあり得るためnullのみチェックする)
            {
                Logger.Error(CLASS_NAME, "Save", "displayText is null!");
                return false;
            }

            try
            {
                TextEditor textEditor = new TextEditor
                {
                    Document = _parent.TextDocument
                };
                textEditor.Save(_parent.DisplayTextFilePath);
            }
            catch (Exception e)
            {
                Logger.Fatal(CLASS_NAME, "Save", e.ToString());
                return false;
            }

            // AvalonEditの編集済みフラグをOffにする
            _parent.ViewModel.IsModified = false;

            return true;
        }

        /// <summary>既存のファイルを読み込みます</summary>
        /// <returns></returns>
        internal bool OpenFile()
        {
            if (IsParentOrViewModelIsNull())
            {
                Logger.Error(CLASS_NAME, "OpenFile", "IsParentOrViewModelIsNull return true!");
                return false;
            }

            // 現在表示中のテキストが編集済みか否かを判定する
            if (_parent.ViewModel.IsModified)
            {
                // 保存されていなければ、Ok/Cancelダイアログを出して確認する
                DialogResult dialogResult = DialogResult.Cancel;
                bool result = DialogManager.GetDialogManager.OpenOkCancelDialog("現在表示中の文章は保存されていません。\n編集を破棄し、新規にファイルを開いて良いですか？\n(※未保存のテキストは破棄されます！)", out dialogResult);
                if (result == false)
                {
                    Logger.Fatal(CLASS_NAME, "OpenFile", $"Dialog error! dialogResult:[{dialogResult}]");
                    return false;
                }

                if (dialogResult == DialogResult.Cancel)
                {
                    Logger.Info(CLASS_NAME, "OpenFile", "Canceled discard text and open new file");
                    return true;
                }
            }

            // 破棄OKか、保存済みであれば現在表示中のテキストとXshdをクリアする
            DisposeTextAndXshd();

            // ファイルを開く
            string openFilePath;
            if (DialogManager.GetDialogManager.OpenFileOpenDialog(out openFilePath) == false)
            {
                Logger.Error(CLASS_NAME, "OpenFile", "OpenFileOpenDialog failed.");
                return false;
            }

            if (string.IsNullOrEmpty(openFilePath))
            {
                Logger.Fatal(CLASS_NAME, "OpenFile", "Filename is null or empty!");
                return false;
            }

            // MainWindowのAvalonEditに適用する
            // 保存したファイルパスを保持する
            _parent.DisplayTextFilePath = openFilePath;

            string loadText;
            if (Load(openFilePath, out loadText) == false)
            {
                Logger.Error(CLASS_NAME, "OpenFile", $"Load error. File path:[{openFilePath}]");
                return false;
            }

            if (_parent.TextDocument == null)
            {
                Logger.Error(CLASS_NAME, "OpenFile", "DisplayTextDocument is null!");
                return false;
            }

            _parent.TextDocument.Text = loadText;

            // 編集済みフラグを下げる
            _parent.ViewModel.IsModified = false;
            _parent.ForceSaveAs = false;

            return true;
        }

        /// <summary>このクラスから辿れる、上位の情報のnullチェックをします</summary>
        /// <returns>true:MainWindowModel, ViewModelいずれかがnull, false:いずれも非null</returns>
        private bool IsParentOrViewModelIsNull()
        {
            if (_parent == null)
            {
                Logger.Fatal(CLASS_NAME, "IsParentOrViewModelIsNull", "_parent is null!");
                return true;
            }

            if (_parent.ViewModel == null)
            {
                Logger.Fatal(CLASS_NAME, "IsParentOrViewModelIsNull", "ViewModel is null!");
                return true;
            }

            return false;
        }


        /// <summary>表示中のテキストと、ハイライト表示情報を破棄します</summary>
        /// <remarks>本当にAvalonEditのDisposeがこれだけで十分かは要検討</remarks>
        private bool DisposeTextAndXshd()
        {
            if (IsParentOrViewModelIsNull())
            {
                Logger.Error(CLASS_NAME, "DisposeTextAndXshd", "IsParentOrViewModelIsNull return true!");
                return false;
            }

            if (_parent.TextDocument == null)
            {
                Logger.Fatal(CLASS_NAME, "DisposeTextAndXshd", "TextDocument is null!");
                return false;
            }

            _parent.TextDocument.Text = string.Empty;

            if (_parent.HighlightManager == null)
            {
                Logger.Fatal(CLASS_NAME, "DisposeTextAndXshd", "HighlightManager is nul!");
                return false;
            }

            _parent.HighlightManager.ResetHighlightInfo();

            return true;
        }


        /// <summary>渡されたファイルパスからテキストファイルを読み込む</summary>
        /// <param name="filePath">読み込み対象のファイルパス</param>
        /// <param name="text">読み込んだファイルの全テキスト</param>
        /// <returns>true:成功, false:失敗</returns>
        private bool Load(string filePath, out string text)
        {
            text = null;

            if (IsParentOrViewModelIsNull())
            {
                Logger.Error(CLASS_NAME, "Load", "IsParentOrViewModelIsNull return true!");
                return false;
            }

            if (string.IsNullOrEmpty(filePath))
            {
                Logger.Error(CLASS_NAME, "Load", "filePath is null or empty!");
                return false;
            }

            _parent.TextDocument.Text = FileAccessor.GetFileAccessor.LoadTextFile(filePath);

            text = _parent.TextDocument.Text;
            return true;
        }


    }
}
