using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Autoinstaller.Helpers
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _executeAction;
        private readonly Func<object, bool> _canExecute;
        private readonly Func<object, Task> _executeAsync;

        public event EventHandler CanExecuteChanged;

        // Синхронный конструктор
        public RelayCommand(Action<object> executeAction, Func<object, bool> canExecute = null)
        {
            _executeAction = executeAction;
            _canExecute = canExecute;
        }

        // Асинхронный конструктор
        public RelayCommand(Func<object, Task> executeAsync, Func<object, bool> canExecute = null)
        {
            _executeAsync = executeAsync;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public async void Execute(object parameter)
        {
            if (_executeAction != null)
            {
                _executeAction(parameter);
            }
            else if (_executeAsync != null)
            {
                await _executeAsync(parameter);
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
