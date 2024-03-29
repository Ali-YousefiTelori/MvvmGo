﻿#if (!NET35 && !NET40)
using MvvmGo.Models;
using MvvmGo.Validations;
using MvvmGo.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
//#if (NETSTANDARD2_0 || NETCOREAPP2_0)
//namespace System.Windows.Input
//{
//    //
//    // Summary:
//    //     Defines a command.
//    public interface ICommand
//    {
//        //
//        // Summary:
//        //     Occurs when changes occur that affect whether or not the command should execute.
//        event EventHandler CanExecuteChanged;

//        //
//        // Summary:
//        //     Defines the method that determines whether the command can execute in its current
//        //     state.
//        //
//        // Parameters:
//        //   parameter:
//        //     Data used by the command. If the command does not require data to be passed,
//        //     this object can be set to null.
//        //
//        // Returns:
//        //     true if this command can be executed; otherwise, false.
//        bool CanExecute(object parameter);
//        //
//        // Summary:
//        //     Defines the method to be called when the command is invoked.
//        //
//        // Parameters:
//        //   parameter:
//        //     Data used by the command. If the command does not require data to be passed,
//        //     this object can be set to null.
//        void Execute(object parameter);
//    }
//}
//#endif
namespace MvvmGo.Commands
{
    public abstract class BaseCommand : ICommand
    {
        bool _IsBusy;
        public bool IsBusy
        {
            get => _IsBusy;
            set => _IsBusy = value;
        }
        public bool IsChangeBusyWhenCommandExecute { get; set; }
        public BaseViewModel BaseViewModel { get; set; }

        public event EventHandler CanExecuteChanged;

        public abstract bool CanExecute(object parameter);

        public abstract void Execute(object parameter);


        public void BaseValidateCanExecute(object value)
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged?.Invoke(value, null);
        }
    }

    public class Command<T> : BaseCommand
    {
        #region Fields

        private readonly Action<T> _execute = null;
        private readonly Predicate<T> _canExecute = null;

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

        public override bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute((T)parameter);
        }

        public override void Execute(object parameter)
        {
            try
            {
                if (IsChangeBusyWhenCommandExecute)
                {
                    BaseViewModel.IsBusy = true;
                }
                IsBusy = true;
                ValidateCanExecute((T)parameter);
                _execute((T)parameter);
            }
            finally
            {
                if (IsChangeBusyWhenCommandExecute)
                {
                    BaseViewModel.IsBusy = false;
                }
                IsBusy = false;
                ValidateCanExecute((T)parameter);
            }
        }

        public void ValidateCanExecute(T value)
        {
            BaseValidateCanExecute(value);
        }


        #endregion // ICommand Members
    }

    public class TaskCommand<T> : BaseCommand
    {
        #region Fields

        private readonly Func<T, Task> _execute = null;
        private readonly Predicate<T> _canExecute = null;

        #endregion // Fields

        #region Constructors

        public TaskCommand(Func<T, Task> execute)
            : this(execute, null)
        {

        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public TaskCommand(Func<T, Task> execute, Predicate<T> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException("please fill execute action");
            _canExecute = canExecute;
        }

        #endregion // Constructors

        #region ICommand Members

        public override bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute((T)parameter);
        }

        public override async void Execute(object parameter)
        {
            try
            {
                if (IsChangeBusyWhenCommandExecute)
                {
                    BaseViewModel.IsBusy = true;
                }
                IsBusy = true;
                ValidateCanExecute((T)parameter);
                await _execute((T)parameter);
            }
            finally
            {
                if (IsChangeBusyWhenCommandExecute)
                {
                    BaseViewModel.IsBusy = false;
                }
                IsBusy = false;
                ValidateCanExecute((T)parameter);
            }

        }

        public void ValidateCanExecute(T value)
        {
            BaseValidateCanExecute(value);
        }
        #endregion // ICommand Members
    }

    public class Command : BaseCommand
    {
        #region Fields

        private Action _execute = null;
        private Func<bool> _canExecute = null;
        private IValidationPropertyChanged _validation = null;
        private ValidationsBuilder _validationsBuilder;
        #endregion // Fields

        #region Constructors

        public Command(Action execute)
            : this(execute, null, null, null)
        {

        }

        public Command(Action execute, IValidationPropertyChanged validation)
            : this(execute, null, null, validation)
        {

        }
        public Command(Action execute, ValidationsBuilder validation)
        {
            GroupValidation(execute, null, null, validation);
        }
        public Command(Action execute, Func<bool> canExecute) : this(execute, canExecute, null, null)
        {

        }

        public Command(Action execute, Func<bool> canExecute, IValidationPropertyChanged validation) : this(execute, canExecute, null, validation)
        {

        }
        public Command(Action execute, Func<ValidationsBuilder, bool> canExecute, ValidationsBuilder validation)
        {
            GroupValidation(execute, canExecute, null, validation);
        }
        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        /// <param name="runBeforeExecute">before run execute action if thst is async this wil run</param>
        public Command(Action execute, Func<bool> canExecute, Action runBeforeExecute) : this(execute, canExecute, runBeforeExecute, null)
        {

        }

        public Command(Action execute, Func<bool> canExecute, Action runBeforeExecute, IValidationPropertyChanged validation)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");
            _validation = validation;
            _execute = () =>
            {
                runBeforeExecute?.Invoke();
                execute();
            };
            _canExecute = canExecute;
        }
        public void GroupValidation(Action execute, Func<ValidationsBuilder, bool> canExecute, Action runBeforeExecute, ValidationsBuilder validation)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");
            _validationsBuilder = validation;
            _execute = () =>
            {
                runBeforeExecute?.Invoke();
                execute();
            };
            _canExecute = () =>
            {
                return canExecute(validation);
            };
        }

        #endregion // Constructors

        #region ICommand Members

        public override bool CanExecute(object parameter = null)
        {
            return _canExecute == null ? true : _canExecute();
        }

        public void Execute()
        {
            if (_validation == null || !_validation.HasError)
                Execute(null);
        }

        public override void Execute(object parameter)
        {
            try
            {
                if (IsChangeBusyWhenCommandExecute)
                {
                    BaseViewModel.IsBusy = true;
                }
                IsBusy = true;
                ValidateCanExecute();
                if (_validation == null || !_validation.HasError)
                    _execute();
            }
            finally
            {
                if (IsChangeBusyWhenCommandExecute)
                {
                    BaseViewModel.IsBusy = false;
                }
                IsBusy = false;
                ValidateCanExecute();
            }
        }

        public void ValidateCanExecute()
        {
            BaseValidateCanExecute(null);
        }
        #endregion // ICommand Members
    }

    public class TaskCommand : BaseCommand
    {
        #region Fields

        private Func<Task> _execute = null;
        private Func<bool> _canExecute = null;
        private IValidationPropertyChanged _validation = null;
        private ValidationsBuilder _validationsBuilder;
        #endregion // Fields

        #region Constructors

        public TaskCommand(Func<Task> execute)
            : this(execute, null, null, null)
        {

        }

        public TaskCommand(Func<Task> execute, IValidationPropertyChanged validation)
            : this(execute, null, null, validation)
        {

        }
        public TaskCommand(Func<Task> execute, ValidationsBuilder validation)
        {
            GroupValidation(execute, null, null, validation);
        }

        public TaskCommand(Func<Task> execute, Func<bool> canExecute) : this(execute, canExecute, null, null)
        {

        }

        public TaskCommand(Func<Task> execute, Func<bool> canExecute, IValidationPropertyChanged validation) : this(execute, canExecute, null, validation)
        {

        }
        public TaskCommand(Func<Task> execute, Func<ValidationsBuilder, bool> canExecute, ValidationsBuilder validation)
        {
            GroupValidation(execute, canExecute, null, validation);
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        /// <param name="runBeforeExecute">before run execute action if thst is async this wil run</param>
        public TaskCommand(Func<Task> execute, Func<bool> canExecute, Action runBeforeExecute) : this(execute, canExecute, runBeforeExecute, null)
        {

        }

        public TaskCommand(Func<Task> execute, Func<bool> canExecute, Action runBeforeExecute, IValidationPropertyChanged validation)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");
            _validation = validation;
            _execute = async () =>
            {
                runBeforeExecute?.Invoke();
                await execute();
            };
            _canExecute = canExecute;
        }

        public void GroupValidation(Func<Task> execute, Func<ValidationsBuilder, bool> canExecute, Action runBeforeExecute, ValidationsBuilder validation)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");
            _validationsBuilder = validation;
            _execute = async () =>
            {
                runBeforeExecute?.Invoke();
                await execute();
            };
            _canExecute = () =>
            {
                return canExecute(validation);
            };
        }

        #endregion // Constructors

        #region ICommand Members

        public override bool CanExecute(object parameter = null)
        {
            return _canExecute == null ? true : _canExecute();
        }

        public void Execute()
        {
            if (_validation == null || !_validation.HasError)
                Execute(null);
        }

        public override async void Execute(object parameter)
        {
            try
            {
                if (IsChangeBusyWhenCommandExecute)
                {
                    BaseViewModel.IsBusy = true;
                }
                IsBusy = true;
                ValidateCanExecute();
                if (_validation == null || !_validation.HasError)
                    await _execute();
            }
            finally
            {
                if (IsChangeBusyWhenCommandExecute)
                {
                    BaseViewModel.IsBusy = false;
                }
                IsBusy = false;
                ValidateCanExecute();
            }
        }

        public void ValidateCanExecute()
        {
            BaseValidateCanExecute(null);
        }
        #endregion // ICommand Members

    }
}
#endif
