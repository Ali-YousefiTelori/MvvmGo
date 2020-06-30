using MvvmGo.Commands;
using MvvmGo.Triggers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace MvvmGo.Triggers
{
    public class CommandSetterInfo : SetterInfoBase
    {
        public IEventCommand Command
        {
            get { return (IEventCommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(IEventCommand), typeof(CommandSetterInfo), new PropertyMetadata(null));

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(CommandSetterInfo), new PropertyMetadata(null));

        public void Fire(params object[] values)
        {
            Command.Execute(CommandParameter, values);
        }
    }
}
