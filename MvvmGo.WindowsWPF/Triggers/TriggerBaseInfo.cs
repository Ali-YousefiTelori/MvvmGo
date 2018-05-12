using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace MvvmGo.Triggers
{
    public class TriggerBaseInfo : DependencyObject
    {
        internal TriggerBaseInfo()
        {

        }

        public bool IsInvert { get; set; }
        public object Value { get; set; }
        public DataConditionCollection Conditions { get; set; } = new DataConditionCollection();
       
        public SetterCollectionInfo Setters { get; set; } = new SetterCollectionInfo();

        public virtual bool Condition(object target)
        {
            bool result = true;
            foreach (var item in Conditions)
            {
                if (item.Type == ConditionType.AndWithPrevious)
                    result = result && item.Condition(target);
                else
                    result = result || item.Condition(target);
            }
            return result;
        }
    }
}
