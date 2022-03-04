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
    /// SettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingWindow : Window
    {

        #region タイトルバーに表示される最大化・最小化・閉じるボタンの非表示化

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            IntPtr handle = new WindowInteropHelper(this).Handle;
            int style = Model.Manager.WindowManager.GetWindowLong(handle, CommonLibrary.Define.GWL_STYLE);
            style &= (~CommonLibrary.Define.WS_SYSMENU);
            Model.Manager.WindowManager.SetWindowLong(handle, CommonLibrary.Define.GWL_STYLE, style);
        }

        #endregion


        public SettingWindow()
        {
            InitializeComponent();
        }
    }
}
