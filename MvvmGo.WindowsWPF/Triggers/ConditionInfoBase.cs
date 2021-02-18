using System;

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
            System.Reflection.PropertyInfo property = target.GetType().GetProperty(propertyName);
            if (property == null)
                throw new Exception($"Property {propertyName} not found on target {target.GetType().FullName}");
            object targetValue = property.GetValue(target, null);
            object sourceValue = Value;

            if (targetValue != null && sourceValue != null && targetValue.GetType() == sourceValue.GetType())
            {
                bool result = targetValue.Equals(sourceValue);
                if (IsInvert)
                    return !result;
                return result;
            }
            else if (targetValue == sourceValue)
            {
                bool result = targetValue == null || targetValue.Equals(sourceValue);
                if (IsInvert)
                    return !result;
                return result;
            }
            else if (sourceValue == null)
            {
                bool result = false;
                if (IsInvert)
                    return !result;
                return result;
            }
            else
            {
                try
                {
                    if (targetValue != null)
                    {
                        if (targetValue.GetType().IsEnum && !sourceValue.GetType().IsEnum)
                            sourceValue = Enum.Parse(targetValue.GetType(), sourceValue.ToString());
                        else
                            sourceValue = Convert.ChangeType(sourceValue, targetValue.GetType());
                    }
                }
                catch (Exception rx)
                {

                }

                bool result = targetValue == null ? targetValue == sourceValue : targetValue.Equals(sourceValue);
                if (IsInvert)
                    return !result;

                return result;
            }
        }

        public object FindSource(object target, string path)
        {
            var split = path.Split('.');
            for (int i = 0; i < split.Length - 1; i++)
            {
                var property = target.GetType().GetProperty(split[i], System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (property != null)
                {
                    target = property.GetValue(target);
                    if (target == null)
                        return null;
                }
                else
                    return null;
            }
            return target;
        }
    }
}
