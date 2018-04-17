using MvvmGo.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace MvvmGo.ViewModels
{
    public class BaseViewModel : IValidationPropertyChanged
    {
        public static void Initialize()
        {
            IsDesignTime = false;
        }

        public static bool IsDesignTime { get; set; } = true;
        public static Action<Action> RunOnUIAction { get; set; }
        public Action<string> PropertyChangedAction { get; set; }

        volatile bool _IsBusy;
        volatile bool _HasError;

        public virtual bool IsBusy
        {
            get => _IsBusy;
            set
            {
                _IsBusy = value;
                OnPropertyChanged(nameof(IsBusy));
            }
        }

#if (!NET35)
        public ObservableCollection<ValidationMessageInfo> AllMessages { get; set; } = new ObservableCollection<ValidationMessageInfo>();
#else
        public Collection<ValidationMessageInfo> AllMessages { get; set; } = new Collection<ValidationMessageInfo>();
#endif
        public bool HasError
        {
            get => _HasError;
            set
            {
                _HasError = value;
                OnPropertyChanged(nameof(HasError));
            }
        }

        public ValidationMessageInfo GetFirstMessage(IEnumerable<ValidationMessageInfo> messageInfos)
        {
            //find first error
            var find = messageInfos.FirstOrDefault(x => x.Type == ValidationMessageType.Error);
            if (find != null)
                return find;
            //find first warning
            find = messageInfos.FirstOrDefault(x => x.Type == ValidationMessageType.Warning);
            if (find != null)
                return find;

            return messageInfos.FirstOrDefault();
        }

        public ValidationMessageInfo FirstMessage
        {
            get
            {
                return GetFirstMessage(AllMessages);
            }
        }

#if (!NET35)
        public System.Collections.Concurrent.ConcurrentDictionary<string, ViewModelItemsInfo> MessagesByProperty { get; set; } = new System.Collections.Concurrent.ConcurrentDictionary<string, ViewModelItemsInfo>();
#else
        public Dictionary<string, ViewModelItemsInfo> MessagesByProperty { get; set; } = new Dictionary<string, ViewModelItemsInfo>();
#endif
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            if (RunOnUIAction != null)
            {
                RunOnUIAction.Invoke(() =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
                    PropertyChangedAction?.Invoke(name);
                });
            }
            else
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
                PropertyChangedAction?.Invoke(name);
            }
        }
    }
}
