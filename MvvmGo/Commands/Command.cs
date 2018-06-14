using MvvmGo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace MvvmGo.Commands
{
#if (!NET35 && !NET40)
    public class Command<T> : ICommand
    {
        #region Fields

        readonly Action<T> _execute = null;
        readonly Predicate<T> _canExecute = null;

        #endregion // Fields

        #region Constructors

        public Command(Action<T> execute)
            : this(execute, null)
        {

        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public Command(Action<T> execute, Predicate<T> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException("please fill execute action");
            _canExecute = canExecute;
        }

        #endregion // Constructors

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute((T)parameter);
        }

        public virtual event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        #endregion // ICommand Members
    }

    public class Command : ICommand
    {
        #region Fields

        Action _execute = null;
        Func<bool> _canExecute = null;
        IValidationPropertyChanged _validation = null;
        private Action<string> changeFileName;

        #endregion // Fields

        #region Constructors

        public Command(Action execute)
            : this(execute, null, null)
        {

        }

        public Command(Action execute, IValidationPropertyChanged validation)
            : this(execute, null, validation)
        {

        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public Command(Action execute, Func<bool> canExecute) : this(execute, canExecute, null)
        {

        }

        public Command(Action execute, Func<bool> canExecute, IValidationPropertyChanged validation)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");
            _validation = validation;
            _execute = execute;
            _canExecute = canExecute;
        }

        #endregion // Constructors

        #region ICommand Members

        public bool CanExecute(object parameter = null)
        {
            return _canExecute == null ? true : _canExecute();
        }

        public virtual event EventHandler CanExecuteChanged;

        public void Execute()
        {
            if (_validation == null || !_validation.HasError)
                Execute(null);
        }

        public void Execute(object parameter)
        {
            if (_validation == null || !_validation.HasError)
                _execute();
        }


        public void ValidateCanExecute()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged?.Invoke(null, null);
        }
        #endregion // ICommand Members

    }
#endif
}
