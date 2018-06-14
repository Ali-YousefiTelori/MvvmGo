using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MvvmGo.Triggers
{
    public enum ConditionType
    {
        AndWithPrevious,
        OrWithPrevious
    }

    public class ConditionInfoBase
    {
        internal ConditionInfoBase()
        {

        }
        public ConditionType Type { get; set; } = ConditionType.AndWithPrevious;
        public bool IsInvert { get; set; }
        public object Value { get; set; }

        public virtual bool Condition(object target)
        {
            throw new NotImplementedException();
        }

        public bool ConditionBase(object target, string propertyName)
        {
            var property = target.GetType().GetProperty(propertyName);
            if (property == null)
                throw new Exception($"Property {propertyName} not found on target {target.GetType().FullName}");
            var targetValue = property.GetValue(target, null);
            var sourceValue = Value;

            if (targetValue.GetType() == sourceValue.GetType())
            {
                var result = targetValue.Equals(sourceValue);
                if (IsInvert)
                    return !result;
                return result;
            }
            else if (targetValue == sourceValue)
            {
                var result = targetValue.Equals(sourceValue);
                if (IsInvert)
                    return !result;
                return result;
            }
            else if (sourceValue == null)
            {
                var result = false;
                if (IsInvert)
                    return !result;
                return result;
            }
            else
            {
                try
                {
                    if (targetValue.GetType().IsEnum && !sourceValue.GetType().IsEnum)
                        sourceValue = Enum.Parse(targetValue.GetType(), sourceValue.ToString());
                    else
                        sourceValue = Convert.ChangeType(sourceValue, targetValue.GetType());

                }
                catch (Exception rx)
                {

                }
                var result = targetValue.Equals(sourceValue);
                if (IsInvert)
                    return !result;
                return result;
            }
        }

    }
}
