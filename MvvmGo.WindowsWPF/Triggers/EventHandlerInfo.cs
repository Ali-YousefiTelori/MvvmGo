using MvvmGo.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MvvmGo.Triggers
{
    public class EventHandlerInfo
    {
        public IEventCommand EventCommand { get; set; }
        public object CommandParameter { get; set; }

        public void Run()
        {
            EventCommand.Execute(CommandParameter);
        }

        public void Run<T1>(T1 value1)
        {
            EventCommand.Execute(CommandParameter, value1);
        }

        public void Run<T1, T2>(T1 value1, T2 value2)
        {
            EventCommand.Execute(CommandParameter, value1, value2);
        }

        public void Run<T1, T2, T3>(T1 value1, T2 value2, T3 value3)
        {
            EventCommand.Execute(CommandParameter, value1, value2, value3);
        }

        public void Run<T1, T2, T3, T4>(T1 value1, T2 value2, T3 value3, T4 value4)
        {
            EventCommand.Execute(CommandParameter, value1, value2, value3, value4);
        }
    }
}
