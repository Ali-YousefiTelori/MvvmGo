using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MvvmGo.Triggers
{
    public class ConditionInfo : ConditionInfoBase
    {
        public string ElementName { get; set; }
        public object Element { get; set; }
        public string Property { get; set; }

        public override bool Condition(object element)
        {
            if (Element != null)
                element = Element;
            var target = element;
            if (ElementName != null)
            {
                target = ((System.Windows.FrameworkElement)target).FindName(ElementName);
            }
            if (target == null)
                throw new Exception($"target by name {ElementName} not found!");

            return ConditionBase(target, Property);
        }
    }
}
