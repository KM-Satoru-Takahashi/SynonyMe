using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SynonyMe.CommonLibrary;
using SynonyMe.View;
using System.Windows;
using SynonyMe.CommonLibrary.Log;
using System.Runtime.InteropServices;

namespace SynonyMe.Model.Manager
{
    /// <summary>サブウィンドウの生成・削除関連の処理を行うクラス</summary>
    internal static class WindowManager
    {
        #region field

        /// <summary>現在画面で表示中のサブウィンドウリスト</summary>
        private static List<SubWindowData> _displaySubWindowList = new List<SubWindowData>();

        private static readonly string CLASS_NAME = "WindowManager";

        #endregion

        #region property

        #endregion

        #region method

        /// <summary>指定されたサブウィンドウの生成を行う</summary>
        /// <param name="subWindowName">対象のウィンドウ名</param>
        internal static void OpenSubWindow(Define.SubWindowName subWindowName)
        {
            switch (subWindowName)
            {
                case Define.SubWindowName.SynonymWindow:
                    CreateSynonymWindow();
                    break;

                case Define.SubWindowName.SettingWindow:
                    CreateSettingWindow();
                    break;

                default:
                    // 想定していないSubWindow名が来ることはあり得ず、対処不能
                    Logger.Fatal(CLASS_NAME, "OpenSubWindow", $"SubWindowName is incorrect! subWindowName:[{subWindowName}]");
                    break;
            }
        }

        private static void CreateSettingWindow()
        {
            if (_displaySubWindowList.Any(w => w.SubWindowName == Define.SubWindowName.SettingWindow))
            {
                // 既に開かれているなら何もせず戻る
                return;
            }

            SettingWindow settingWindow = new SettingWindow();
            _displaySubWindowList.Add(new SubWindowData(settingWindow, Define.SubWindowName.SettingWindow));

            settingWindow.ShowDialog();
        }

        /// <summary>類語登録ウィンドウを開く</summary>
        private static void CreateSynonymWindow()
        {
            if (_displaySubWindowList.Any(w => w.SubWindowName == Define.SubWindowName.SynonymWindow))
            {
                // 既に開かれているなら何もせず戻る
                return;
            }

            SynonymWindow synonymWindow = new SynonymWindow();
            _displaySubWindowList.Add(new SubWindowData(synonymWindow, Define.SubWindowName.SynonymWindow));

            synonymWindow.ShowDialog();
        }

        /// <summary>対象のウィンドウを閉じる</summary>
        /// <param name="targetWindow"></param>
        internal static void CloseSubWindow(Define.SubWindowName targetWindow)
        {
            if (_displaySubWindowList.Any(w => w.SubWindowName == targetWindow) == false)
            {
                // 管理外ならなにもしない
                return;
            }

            SubWindowData targetWindowData = _displaySubWindowList.Find(w => w.SubWindowName == targetWindow);
            if (targetWindowData.Window != null)
            {
                _displaySubWindowList.Remove(targetWindowData);
                targetWindowData.Window.Close();
            }
        }

        /// <summary>表示中のサブWindowを取得する</summary>
        /// <param name="windowName"></param>
        /// <returns></returns>
        internal static Window GetSubWindow(Define.SubWindowName windowName)
        {
            if (_displaySubWindowList == null || _displaySubWindowList.Any() == false)
            {
                return null;
            }

            return _displaySubWindowList.Find(w => w.SubWindowName == windowName).Window;
        }

        /// <summary>
        /// MainWindowを取得する
        /// </summary>
        /// <returns>可能ならGetSubWindowと統合させるべき……</returns>
        internal static MainWindow GetMainWindow()
        {
            Window view = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w is MainWindow);
            MainWindow mw = view as MainWindow;
            if (mw == null)
            {
                Logger.Fatal(CLASS_NAME, "GetMainWindow", "MainWindow is null");
                return null;
            }

            return mw;
        }

        #endregion

        #region import method

        [DllImport("user32.dll")]
        internal static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        internal static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        #endregion

        #region struct

        /// <summary>サブウィンドウを管理する構造体</summary>
        private struct SubWindowData
        {
            internal SubWindowData(Window window, Define.SubWindowName subWindowName)
            {
                Window = window;
                SubWindowName = subWindowName;
            }

            /// <summary>サブウィンドウ画面</summary>
            internal Window Window { get; private set; }

            /// <summary>サブウィンドウ名</summary>
            internal Define.SubWindowName SubWindowName { get; private set; }
        }

        #endregion
    }
}
