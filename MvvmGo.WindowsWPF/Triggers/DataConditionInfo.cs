﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace MvvmGo.Triggers
{
    public class DataConditionInfo : ConditionInfoBase
    {
        public BindingBase Binding { get; set; }
        internal INotifyPropertyChanged PropertyChanged { get; set; }
        public override bool Condition(object target)
        {
            var binding = (System.Windows.Data.Binding)Binding;
            if (binding == null)
                return false;
            string propertyName = binding.Path.Path.Split('.').LastOrDefault();
            var source = binding.Source;
            if (source == null)
                source = PropertyChanged;

            if (source == null)
            {
                if (target is FrameworkElement element)
                {
                    if (element.DataContext != null)
                    {
                        source = FindSource(element.DataContext, binding.Path.Path);
                    }
                }
            }

            if (source == null || source == null)
                return false;
            return ConditionBase(source, propertyName);
        }
        
    }
}
