using System;
using System.Windows.Input;

namespace Autoinstaller.Helpers
{
    internal class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        /// <summary>
        /// Создаёт команду, которая всегда может выполняться.
        /// </summary>
        /// <param name="execute">Действие, выполняемое командой.</param>
        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Создаёт команду, у которой можно указать условие выполнения.
        /// </summary>
        /// <param name="execute">Действие, выполняемое командой.</param>
        /// <param name="canExecute">Функция, определяющая, можно ли выполнить команду.</param>
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        /// <summary>
        /// Вызывайте этот метод, чтобы обновить состояние возможности выполнения команды (CanExecute).
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
