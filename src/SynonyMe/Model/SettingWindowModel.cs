using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynonyMe.Model
{
    /// <summary>設定ウィンドウの内部処理を担います</summary>
    internal class SettingWindowModel
    {
        private ViewModel.SettingWindowVM _viewModel = null;

        internal SettingWindowModel(ViewModel.SettingWindowVM vm)
        {
            _viewModel = vm;
        }
    }
}
