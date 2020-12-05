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
        private static readonly Action<string> EmptyExecute = (param) => { };
        private static readonly Func<bool> EmptyCanExecute = () => true;

        private Action<string> execute;
        private Func<bool> canExecute;

        public CommandBase(Action<string> execute, Func<bool> canExecute = null)
        {
            this.execute = execute ?? EmptyExecute;
            this.canExecute = canExecute ?? EmptyCanExecute;
        }

        public void Execute(string param = null)
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
            this.Execute(parameter as string);
        }
    }

    static class CommandExetensions
    {
        public static void RaiseCanExecuteChanged(this ICommand self)
        {
            var CommandBase = self as CommandBase;
            if (CommandBase == null)
            {
                return;
            }

            CommandBase.RaiseCanExecuteChanged();
        }

        public static void RaiseCanExecuteChanged(this IEnumerable<ICommand> self)
        {
            foreach (var command in self)
            {
                command.RaiseCanExecuteChanged();
            }
        }

        public static void RaiseCanExecuteChanged(params ICommand[] commands)
        {
            commands.RaiseCanExecuteChanged();
        }

    }
}
