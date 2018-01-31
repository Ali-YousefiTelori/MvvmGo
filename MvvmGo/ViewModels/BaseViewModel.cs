using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace MvvmGo.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public static Action<Action> RunOnUIAction { get; set; }
        public Action<string> PropertyChangedAction { get; set; }

        volatile bool _IsBusy;

        public virtual bool IsBusy
        {
            get => _IsBusy;
            set
            {
                _IsBusy = value;
                OnPropertyChanged("IsBusy");
            }
        }

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
