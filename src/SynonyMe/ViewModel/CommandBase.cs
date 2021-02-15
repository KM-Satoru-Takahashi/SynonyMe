using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SynonyMe.ViewModel
{
    /// <summary>ボタン押下時のコマンド基底クラス</summary>
    internal class CommandBase : ICommand
    {
        private static readonly Action<object> EmptyExecute = (param) => { };
        private static readonly Func<bool> EmptyCanExecute = () => true;

        private Action<object> execute;
        private Func<bool> canExecute;

        public CommandBase(Action<object> execute, Func<bool> canExecute = null)
        {
            this.execute = execute ?? EmptyExecute;
            this.canExecute = canExecute ?? EmptyCanExecute;
        }

        public void Execute(object param = null)
        {
            this.execute(param);
        }

        public bool CanExecute()
        {
            return this.canExecute();
        }

        bool ICommand.CanExecute(object parameter)
        {
            return this.CanExecute();
        }

        public event EventHandler CanExecuteChanged;
        public void RaiseCanExecuteChanged()
        {
            var h = this.CanExecuteChanged;
            if (h != null)
            {
                //h(this, EventArgs.Empty); 
                h(null, null);
            }
        }

        void ICommand.Execute(object parameter)
        {
            this.Execute(parameter);
        }
    }
}
