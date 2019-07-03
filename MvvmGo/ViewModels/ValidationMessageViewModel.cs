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
        public Action OnValidationChanged { get; set; }
        public static List<Action> AllPropertyChanges { get; set; } = new List<Action>();

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
            OnValidationChanged?.Invoke();
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


        public static void AllValidationChanged()
        {
            foreach (var item in AllPropertyChanges)
            {
                item();
            }
        }
    }
}
