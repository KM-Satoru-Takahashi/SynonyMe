using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SynonyMe.View
{
    /// <summary>
    /// SynonymWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SynonymWindow : Window
    {
        #region タイトルバーに表示される最大化・最小化・閉じるボタンの非表示化

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        protected override void OnSourceInitialized(EventArgs e)
        {
            // 固定値の定義
            const int GWL_STYLE = -16;
            const int WS_SYSMENU = 0x80000;

            base.OnSourceInitialized(e);
            IntPtr handle = new WindowInteropHelper(this).Handle;
            int style = GetWindowLong(handle, GWL_STYLE);
            style &= (~WS_SYSMENU);
            SetWindowLong(handle, GWL_STYLE, style);
        }

        #endregion

        public SynonymWindow()
        {
            InitializeComponent();
        }
    }
}
