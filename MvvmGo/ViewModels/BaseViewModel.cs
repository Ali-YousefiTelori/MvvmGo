using MvvmGo.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MvvmGo.ViewModels
{
    public class BaseViewModel : PropertyChangedViewModel, IValidationPropertyChanged
    {
        public static void Initialize()
        {
            IsDesignTime = false;
        }

        public static bool IsDesignTime { get; set; } = true;
        public static Action<Action> RunOnUIAction { get; set; }
        public Action<string> PropertyChangedAction { get; set; }
        /// <summary>
        /// when busy changed
        /// </summary>
        public Action<bool, string> IsBusyChangedAction { get; set; }
        public Action<string> BusyContentChangedAction { get; set; }

        private bool _IsBusy;
        private string _BusyContent;
        private bool _HasError;

        public virtual bool IsBusy
        {
            get
            {
                return _IsBusy;
            }

            set
            {
                _IsBusy = value;
                OnPropertyChanged(nameof(IsBusy));
                IsBusyChangedAction?.Invoke(_IsBusy, BusyContent);
            }
        }

        public string BusyContent
        {
            get
            {
                return _BusyContent;
            }
            set
            {
                _BusyContent = value;
                OnPropertyChanged(nameof(BusyContent));
                BusyContentChangedAction?.Invoke(BusyContent);
            }
        }
#if (!NET35)
        public ObservableCollection<ValidationMessageInfo> AllMessages { get; set; } = new ObservableCollection<ValidationMessageInfo>();
#else
        public Collection<ValidationMessageInfo> AllMessages { get; set; } = new Collection<ValidationMessageInfo>();
#endif
        public bool HasError
        {
            get
            {
                return _HasError;
            }

            set
            {
                _HasError = value;
                OnPropertyChanged(nameof(HasError));
            }
        }

        public ValidationMessageInfo GetFirstMessage(IEnumerable<ValidationMessageInfo> messageInfos)
        {
            //find first error
            ValidationMessageInfo find = messageInfos.FirstOrDefault(x => x.Type == ValidationMessageType.Error);
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

        public override void OnPropertyChanged(string name)
        {
            if (RunOnUIAction != null)
            {
                RunOnUIAction.Invoke(() =>
                {
                    base.OnPropertyChanged(name);
                    PropertyChangedAction?.Invoke(name);
                });
            }
            else
            {
                base.OnPropertyChanged(name);
                PropertyChangedAction?.Invoke(name);
            }
        }
    }
}
