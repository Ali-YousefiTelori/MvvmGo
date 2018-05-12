using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace MvvmGo.Triggers
{
    public class DataSetterInfo : SetterInfoBase
    {
        public BindingBase Binding { get; set; }
        public string Property { get; set; }
        internal object DefaultValue { get; set; }

        public object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(DataSetterInfo), new PropertyMetadata(null));

        public override object SetValue(object target)
        {
            var binding = ((Binding)Binding);
            //var allPathes = binding.Path.Path.Split('.');

            object context = binding.Source;
            if (context == null)
                context = ((FrameworkElement)target).DataContext;

            //var property = context.GetType().GetProperty(Property);
            
            return SetBaseValue(context, Property, Value);
        }
    }
}