using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynonyMe.Model.MainWindow
{
    internal class ToolbarModel
    {
        private MainWindowModel _parent=null;

        public ToolbarModel(MainWindowModel parent)
        {
            _parent=parent;
        }
    }
}
