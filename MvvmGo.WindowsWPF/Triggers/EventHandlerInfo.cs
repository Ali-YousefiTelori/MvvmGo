using MvvmGo.Commands;

namespace MvvmGo.Triggers
{
    public class EventHandlerInfo
    {
        public EventSetterInfo EventSetterInfo { get; set; }
        public IEventCommand EventCommand { get; set; }
        public object CommandParameter { get; set; }
        public object Target { get; set; }

        public void Run()
        {
            //if (CommandParameter == null)
            SetCommandParameter();
            EventCommand.Execute(CommandParameter);
        }

        public void Run<T1>(T1 value1)
        {
            //if (CommandParameter == null)
            SetCommandParameter();
            EventCommand.Execute(CommandParameter, value1);
        }

        public void Run<T1, T2>(T1 value1, T2 value2)
        {
            //if (CommandParameter == null)
            SetCommandParameter();
            EventCommand.Execute(CommandParameter, value1, value2);
        }

        public void Run<T1, T2, T3>(T1 value1, T2 value2, T3 value3)
        {
            //if (CommandParameter == null)
            SetCommandParameter();
            EventCommand.Execute(CommandParameter, value1, value2, value3);
        }

        public void Run<T1, T2, T3, T4>(T1 value1, T2 value2, T3 value3, T4 value4)
        {
            //if (CommandParameter == null)
            SetCommandParameter();
            EventCommand.Execute(CommandParameter, value1, value2, value3, value4);
        }

        private void SetCommandParameter()
        {
            object parameterValue = EventSetterInfo.GetValue(EventSetterInfo.CommandParameterProperty);
            if (parameterValue == null)
            {
                System.Windows.Data.Binding binding = System.Windows.Data.BindingOperations.GetBinding(EventSetterInfo, EventSetterInfo.CommandParameterProperty);
                if (binding != null)
                {
                    if (binding.Path == null)
                    {
                        parameterValue = ((System.Windows.FrameworkElement)Target).DataContext;
                    }
                    else
                    {
                        parameterValue = TriggerExtensions.GetValueBinding(binding, ((System.Windows.FrameworkElement)Target));
                    }
                }
            }
            CommandParameter = parameterValue;
        }
    }
}
