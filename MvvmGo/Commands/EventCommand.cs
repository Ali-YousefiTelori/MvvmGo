using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace MvvmGo.Commands
{
    public interface IEventCommand
    {
        void Execute(object parameter, params object[] values);
    }

    public class EventCommand<T> : IEventCommand
    {
        Action<T, object[]> _execute;
        Action<T> _execute2;
        public EventCommand(Action<T, object[]> execute)
        {
            _execute = execute;
        }

        public EventCommand(Action<T> execute)
        {
            _execute2 = execute;
        }

        public void Execute(T parameter, params object[] values)
        {
            if (_execute2 == null)
                _execute?.Invoke(parameter, values);
            else
                _execute2?.Invoke(parameter);
        }

        public void Execute(object parameter, params object[] values)
        {
            Execute((T)parameter, values);
        }
    }

    public class EventCommand : IEventCommand
    {
        Action<object[]> _execute;
        Action _execute2;
        public EventCommand(Action<object[]> execute)
        {
            _execute = execute;
        }

        public EventCommand(Action execute)
        {
            _execute2 = execute;
        }

        public void Execute(params object[] values)
        {
            if (_execute2 == null)
                _execute?.Invoke(values);
            else
                _execute2?.Invoke();
        }

        public void Execute(object parameter, params object[] values)
        {
            Execute(values);
        }
    }
}
