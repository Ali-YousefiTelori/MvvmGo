using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace MvvmGo.Triggers
{
    public class DataTriggerInfo : TriggerBaseInfo
    {
        public BindingBase Binding { get; set; }
    }
}
