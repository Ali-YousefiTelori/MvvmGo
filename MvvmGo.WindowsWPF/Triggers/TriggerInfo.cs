using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace MvvmGo.Triggers
{
    public class TriggerInfo : TriggerBaseInfo
    {
        public FrameworkElement TargetElement
        {
            get { return (FrameworkElement)GetValue(TargetElementProperty); }
            set { SetValue(TargetElementProperty, value); }
        }


        public static readonly DependencyProperty TargetElementProperty =
            DependencyProperty.Register("TargetElement", typeof(FrameworkElement), typeof(DataTriggerInfo), new PropertyMetadata(null));


        public string TargetName { get; set; }
        public string Property { get; set; }

    }
}
