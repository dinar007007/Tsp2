using System;
using System.Windows.Input;

namespace Tsp.Wpf.Common
{
    public class DelegateCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action<object> act) : this(act, (o) => true)
        {
        }

        public DelegateCommand(Action act) : this(o => act())
        {
        }

        public DelegateCommand(Action<object> act, Func<object, bool> canExecute)
        {
            _execute = act;
            _canExecute = canExecute;
        }


        public bool CanExecute(object parameter)
        {
            return _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}
