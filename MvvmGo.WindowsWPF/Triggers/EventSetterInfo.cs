using MvvmGo.Commands;
using System;
using System.Windows;

namespace MvvmGo.Triggers
{
    public class EventSetterInfo : SetterInfoBase
    {
        public string ElementName { get; set; }
        public string EventName { get; set; }

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
            DependencyProperty.Register("Command", typeof(IEventCommand), typeof(EventSetterInfo), new PropertyMetadata(null));

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(EventSetterInfo), new PropertyMetadata(null));
        internal Delegate Handler { get; set; }

        public override object SetValue(object target)
        {
            if (ElementName != null)
            {
                target = ((System.Windows.FrameworkElement)target).FindName(ElementName);
            }

            if (target == null)
                return null;

            var eventInfo = target.GetType().GetEvent(EventName);
            if (eventInfo != null)
            {
                TriggerExtensions.RemoveEvent(target, eventInfo, this);
                TriggerExtensions.SetEvent(target, eventInfo, this);
            }
            return null;
        }

        public override object SetCustomValue(object target, object value)
        {
            if (ElementName != null)
            {
                target = ((System.Windows.FrameworkElement)target).FindName(ElementName);
            }

            if (target == null)
                return null;

            var eventInfo = target.GetType().GetEvent(EventName);
            if (eventInfo != null)
            {
                TriggerExtensions.RemoveEvent(target, eventInfo, this);
            }
            return null;
        }
    }
}
