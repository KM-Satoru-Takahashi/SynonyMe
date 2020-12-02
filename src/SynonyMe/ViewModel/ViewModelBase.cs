using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SynonyMe.ViewModel
{
    /// <summary>
    /// ViewModelの基底クラス
    /// INotifyPropertyChanged と IDataErrorInfo を実装する
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged, IDataErrorInfo
    {
        #region INotifyPropertyChanged

        // INotifyPropertyChanged.PropertyChanged の実装
        public event PropertyChangedEventHandler PropertyChanged;

        // INotifyPropertyChanged.PropertyChangedイベントを発生させる。
        protected virtual void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region IDataErrorInfo

        // IDataErrorInfo用のエラーメッセージを保持する辞書
        private Dictionary<string, string> _errorMessages = new Dictionary<string, string>();

        // IDataErrorInfo.Error の実装
        string IDataErrorInfo.Error
        {
            get { return (_errorMessages.Count > 0) ? "Has Error" : null; }
        }

        // IDataErrorInfo.Item の実装
        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                if (_errorMessages.ContainsKey(columnName))
                {
                    return _errorMessages[columnName];
                }
                else
                {
                    return null;
                }
            }
        }

        // エラーメッセージのセット
        protected void SetError(string propertyName, string errorMessage)
        {
            _errorMessages[propertyName] = errorMessage;
        }

        // エラーメッセージのクリア
        protected void ClearErrror(string propertyName)
        {
            if (_errorMessages.ContainsKey(propertyName))
                _errorMessages.Remove(propertyName);
        }

        #endregion

        #region ICommand Helper

        // ICommand実装用のヘルパークラス
        private class DelegateCommand : ICommand
        {
            private Action<object> _command;        // コマンド本体
            private Func<object, bool> _canExecute;  // 実行可否

            // コンストラクタ
            public DelegateCommand(Action<object> command, Func<object, bool> canExecute = null)
            {
                if (command == null)
                {
                    throw new ArgumentNullException();
                }

                _command = command;
                _canExecute = canExecute;
            }

            // ICommand.Executeの実装
            void ICommand.Execute(object parameter)
            {
                _command(parameter);
            }

            // ICommand.Executeの実装
            bool ICommand.CanExecute(object parameter)
            {
                if (_canExecute != null)
                {
                    return _canExecute(parameter);
                }
                else
                {
                    return true;
                }
            }

            // ICommand.CanExecuteChanged の実装
            event EventHandler ICommand.CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }
        }        

        // コマンドの生成
        protected ICommand CreateCommand(Action<object> command, Func<object, bool> canExecute = null)
        {
            return new DelegateCommand(command, canExecute);
        }

        #endregion
    }
}
