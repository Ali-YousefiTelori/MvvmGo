using MvvmGo.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace MvvmGo.Models
{
    public class ViewModelItemsInfo
    {
        public ValidationMessageViewModel ViewModel { get; set; }
#if (!NET35)
        public ObservableCollection<ValidationMessageInfo> Items { get; set; }
#else
        public Collection<ValidationMessageInfo> Items { get; set; }
#endif

    }
}
