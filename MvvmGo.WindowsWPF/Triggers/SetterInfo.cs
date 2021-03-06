﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace MvvmGo.Triggers
{
    public class SetterInfo : SetterInfoBase
    {
        public string ElementName { get; set; }

        public object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(SetterInfo), new PropertyMetadata(null));

        public string Property { get; set; }
        internal object DefaultValue { get; set; }

        public override object SetValue(object target)
        {
            return SetCustomValue(target, Value);
        }

        public override object SetCustomValue(object target, object value)
        {
            if (ElementName != null)
            {
                target = ((System.Windows.FrameworkElement)target).FindName(ElementName);
            }

            if (target == null)
                return null;
            var property = target.GetType().GetProperty(Property);
            if (property == null)
                return null;
            value = ConvertValue(value, property);
            property.SetValue(target, value, null);
            return value;
        }
    }
}
