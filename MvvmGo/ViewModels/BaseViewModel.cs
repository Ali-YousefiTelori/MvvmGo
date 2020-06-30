using MvvmGo.Commands;
using MvvmGo.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace MvvmGo.ViewModels
{
    public class BaseViewModel : PropertyChangedViewModel, IValidationPropertyChanged
    {
#if (!NET35 && !NET40)
        public bool IsChangeBusyWhenCommandExecute { get; set; }
        List<BaseCommand> Commands { get; set; } = new List<BaseCommand>();
        public void InitializeCommands()
        {
#if (NETSTANDARD1_6)
            throw new NotSupportedException();
#else
            foreach (var property in this.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).Where(x => x.PropertyType.BaseType == typeof(BaseCommand)))
            {
                var command = (BaseCommand)property.GetValue(this);
                if (command != null)
                {
                    command.IsChangeBusyWhenCommandExecute = IsChangeBusyWhenCommandExecute;
                    command.BaseViewModel = this;
                    Commands.Add(command);
                }
            }
#endif
        }
#endif
        public void ValidateAllCommands()
        {
#if (!NET35 && !NET40)
            if (RunOnUIAction != null)
            {
                RunOnUIAction.Invoke(() =>
                {
                    foreach (var item in Commands)
                    {
                        item.BaseValidateCanExecute(null);
                    }
                });
            }
            else
            {
                foreach (var item in Commands)
                {
                    item.BaseValidateCanExecute(null);
                }
            }
            
#else
            throw new NotSupportedException();
#endif
        }

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
#if (!NET35 && !NET40)
                if (Commands.Count > 0)
                {
                    ValidateAllCommands();
                }
#endif
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
                ValidateCanExecute();
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

        public void ValidateCanExecute()
        {
#if (!NET35 && !NET40 && !NETSTANDARD1_6)
            foreach (var item in this.GetType().GetProperties())
            {
                if (item.PropertyType == typeof(Command))
                {
                    var pValue = item.GetValue(this, null);
                    var method = item.PropertyType.GetMethod("ValidateCanExecute");
                    if (pValue != null)
                        method.Invoke(pValue, null);
                }
            }
#endif
        }
    }
}
