using MvvmGo.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace MvvmGo.Models
{
    public interface IValidationPropertyChanged
    {
        ValidationMessageInfo FirstMessage { get; }
        ValidationMessageInfo GetFirstMessage(IEnumerable<ValidationMessageInfo> messageInfos);
#if (!NET35)
        ObservableCollection<ValidationMessageInfo> AllMessages { get; set; }
#else
        Collection<ValidationMessageInfo> AllMessages { get; set; }
#endif

#if (!NET35)
        System.Collections.Concurrent.ConcurrentDictionary<string, ViewModelItemsInfo> MessagesByProperty { get; set; }
#else
        Dictionary<string, ViewModelItemsInfo> MessagesByProperty { get; set; }
#endif
        bool HasError { get; set; }
        void OnPropertyChanged(string name);
        void ValidateCanExecute();
    }
}
