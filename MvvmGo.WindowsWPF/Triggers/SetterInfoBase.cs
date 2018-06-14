using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

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
            value = ConvertValue(value, property);
            property.SetValue(target, value, null);
            return value;
        }

        static bool HasOnBase(object value, Type baseType)
        {
            var type = value.GetType();
            do
            {
                if (type == baseType)
                    return true;
                type = type.BaseType;
            }
            while (type != null);
            return false;
        }

        public static object ConvertValue(object value, PropertyInfo property)
        {
            if (value == null || HasOnBase(value, property.PropertyType))
            {
                return value;
            }

            try
            {
                if (property.PropertyType.IsEnum)
                    value = Enum.Parse(property.PropertyType, value.ToString());
                else if (property.PropertyType == typeof(Cursor) && !HasOnBase(value, typeof(Cursor)))
                {
                    value = typeof(Cursors).GetProperty(value.ToString()).GetValue(null, null);
                }
                else if (property.PropertyType == typeof(Brush) && !HasOnBase(value, typeof(Brush)))
                {
                    BrushConverter converter = new BrushConverter();
                    value = converter.ConvertFromString(value.ToString());
                }
                //else if (property.PropertyType == typeof(TextDecorationCollection) && !HasOnBase(value, typeof(TextDecorationCollection)))
                //{
                //    var parse = (TextDecorationLocation)Enum.Parse(typeof(TextDecoration), value.ToString());
                //    System.Windows.Controls.TextBlock textBlock;
                //    value = new TextDecorationCollection(new List<TextDecoration>() { new TextDecoration(parse) });
                //                  { Name = "TextDecorationCollection" FullName = "System.Windows.TextDecorationCollection"}
                //    BrushConverter converter = new BrushConverter();
                //    value = converter.ConvertFromString(value.ToString());
                //}
                else if (Nullable.GetUnderlyingType(property.PropertyType) != null)
                {
                    value = Convert.ChangeType(value, property.PropertyType.GetGenericArguments()[0]);
                }
                else
                    value = Convert.ChangeType(value, property.PropertyType);
            }
            catch (Exception rx)
            {

            }
            return value;
        }
    }
}
