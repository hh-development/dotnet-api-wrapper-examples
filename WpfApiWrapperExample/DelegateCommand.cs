using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WpfApiWrapperExample
{
    /// <summary>
    /// Simple implementation of the DelegateCommand (aka RelayCommand) pattern.
    /// </summary>
    public class DelegateCommand : DelegateCommand<object>
    {
        /// <summary>
        /// Initializes a new instance of the DelegateCommand class.
        /// </summary>
        /// <param name="execute">Action to invoke when Execute is called.</param>
        public DelegateCommand(Action<object> execute) : base(execute)
        {

        }

        /// <summary>
        /// Initializes a new instance of the DelegateCommand class.
        /// </summary>
        public DelegateCommand(Action execute) : base(execute)
        {

        }

        /// <summary>
        /// Initializes a new instance of the DelegateCommand class.
        /// </summary>
        /// <param name="canExecute">Func to invoke when CanExecute is called.</param>
        /// <param name="execute">Action to invoke when Execute is called.</param>
        public DelegateCommand(Func<object, bool> canExecute, Action<object> execute) : base(canExecute, execute)
        {

        }

        /// <summary>
        /// Initializes a new instance of the DelegateCommand class.
        /// </summary>
        /// <param name="canExecute">Func to invoke when CanExecute is called.</param>
        /// <param name="execute">Action to invoke when Execute is called.</param>
        public DelegateCommand(Func<object, bool> canExecute, Action execute) : base(canExecute, execute)
        {

        }
    }

    /// <summary>
    /// Simple implementation of the DelegateCommand (aka RelayCommand) pattern.
    /// </summary>
    public class DelegateCommand<T> : ICommand
    {
        /// <summary>
        /// Stores a reference to the CanExecute Func.
        /// </summary>
        private readonly Func<T, bool> _canExecute;

        /// <summary>
        /// Stores a reference to the Execute Action.
        /// </summary>
        private readonly Action<T> _execute;

        /// <summary>
        /// Stores a reference to the Execute Action.
        /// </summary>
        private readonly Action _executeNoParam;

        /// <summary>
        /// Initializes a new instance of the DelegateCommand class.
        /// </summary>
        /// <param name="execute">Action to invoke when Execute is called.</param>
        public DelegateCommand(Action<T> execute)
            : this(param => true, execute)
        {
        }

        /// <summary>
        /// Initializes a new instance of the DelegateCommand class.
        /// </summary>
        public DelegateCommand(Action execute)
            : this(param => true, execute)
        {
        }

        /// <summary>
        /// Initializes a new instance of the DelegateCommand class.
        /// </summary>
        /// <param name="canExecute">Func to invoke when CanExecute is called.</param>
        /// <param name="execute">Action to invoke when Execute is called.</param>
        public DelegateCommand(Func<T, bool> canExecute, Action<T> execute)
        {
            Debug.Assert(canExecute != null, "canExecute must not be null.");
            Debug.Assert(execute != null, "execute must not be null.");
            _canExecute = canExecute;
            _execute = execute;
        }

        /// <summary>
        /// Initializes a new instance of the DelegateCommand class.
        /// </summary>
        /// <param name="canExecute">Func to invoke when CanExecute is called.</param>
        /// <param name="execute">Action to invoke when Execute is called.</param>
        public DelegateCommand(Func<T, bool> canExecute, Action execute)
        {
            Debug.Assert(canExecute != null, "canExecute must not be null.");
            Debug.Assert(execute != null, "execute must not be null.");
            _canExecute = canExecute;
            _executeNoParam = execute;
        }

        /// <summary>
        /// Determines if the command can execute.
        /// </summary>
        /// <param name="parameter">Data used by the command.</param>
        /// <returns>True if the command can execute.</returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute((T)parameter);
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="parameter">Data used by the command.</param>
        public void Execute(object parameter = null)
        {
            if (_executeNoParam != null)
                _executeNoParam();
            else
                _execute((T)parameter);
        }

        /// <summary>
        /// Invoked when changes to the execution state of the command occur.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Invokes the CanExecuteChanged event.
        /// </summary>
        public void OnCanExecuteChanged()
        {
            EventHandler handler = CanExecuteChanged;
            if (null != handler)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}
