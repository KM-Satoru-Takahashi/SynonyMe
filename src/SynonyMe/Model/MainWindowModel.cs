using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.ObjectModel;
using System.IO;
using GongSolutions.Wpf.DragDrop;

namespace SynonyMe.Model
{
    internal class MainWindowModel
    {

        #region field

        private SynonyMe.ViewModel.MainWindowVM _viewModel = null;

        #endregion

        #region method

        internal MainWindowModel(ViewModel.MainWindowVM viewModel)
        {
            _viewModel = viewModel;
        }

        #endregion
    }
}
