using MvvmGo.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace MvvmGo.ViewModels
{
    public class ValidationMessageViewModel : INotifyPropertyChanged
    {
        public string PropertyName { get; set; }
        public IValidationPropertyChanged CurrentViewModel { get; set; }
        public ValidationMessageInfo FirstMessage
        {
            get
            {
                if (CurrentViewModel == null || !CurrentViewModel.MessagesByProperty.ContainsKey(PropertyName))
                    return null;
                return CurrentViewModel.GetFirstMessage(CurrentViewModel.MessagesByProperty[PropertyName].Items);
            }
        }
        
        public bool HasError
        {
            get
            {
                return FirstMessage != null;
            }
        }

        public void Validate()
        {
            OnPropertyChanged(nameof(HasError));
            OnPropertyChanged(nameof(FirstMessage));
        }

        public void OnPropertyChanged(string name)
        {
            if (BaseViewModel.RunOnUIAction != null)
            {
                BaseViewModel.RunOnUIAction.Invoke(() =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
                });
            }
            else
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
