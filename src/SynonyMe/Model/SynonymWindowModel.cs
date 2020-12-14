using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SynonyMe.ViewModel;

namespace SynonyMe.Model
{
    internal class SynonymWindowModel
    {
        #region field

        /// <summary>ViewModel</summary>
        private SynonymWindowVM _vm = null;

        #endregion

        #region property

        #endregion

        #region method

        /// <summary>コンストラクタ</summary>
        /// <param name="vm"></param>
        internal SynonymWindowModel(SynonymWindowVM vm)
        {
            _vm = vm;
        }


        internal void CloseSynonymWindow()
        {
            WindowManager.CloseSubWindow(CommonLibrary.Define.SubWindowName.SynonymWindow);
        }

        #endregion
    }
}
