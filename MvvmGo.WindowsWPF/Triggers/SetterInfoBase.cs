using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace MvvmGo.Triggers
{
    public class SetterInfoBase : DependencyObject
    {
        public virtual object SetValue(object target)
        {
            throw new NotImplementedException();
        }

        public virtual object SetCustomValue(object target, object value)
        {
            throw new NotImplementedException();
        }

        public static object SetBaseValue(object target, string propertyName, object value)
        {
            var property = target.GetType().GetProperty(propertyName);

            if (value == null || property.PropertyType == value.GetType())
            {
                property.SetValue(target, value, null);
                return value;
            }

            try
            {
                if (property.PropertyType.IsEnum)
                    value = Enum.Parse(property.PropertyType, value.ToString());
                else if (property.PropertyType == typeof(Cursor) && value.GetType() != typeof(Cursor))
                {
                    value = typeof(Cursors).GetProperty(value.ToString()).GetValue(null, null);
                }
                else if (Nullable.GetUnderlyingType(property.PropertyType) != null)
                {
                    value = Convert.ChangeType(value, property.PropertyType.GetGenericArguments()[0]);
                }
                else
                    value = Convert.ChangeType(value, property.PropertyType);
                property.SetValue(target, value, null);
            }
            catch (Exception rx)
            {

            }
            return value;
        }
    }
}
