using MvvmGo.Attributes;
using MvvmGo.Models;
using MvvmGo.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace MvvmGo.Validations
{
    public class ValidationsHelper
    {
        public static readonly DependencyProperty PropertyProperty = DependencyProperty.RegisterAttached(
            "Property",
            typeof(string),
            typeof(ValidationsHelper), new PropertyMetadata(new PropertyChangedCallback(OnPropertyPropertyChanged)));


        public static string GetProperty(FrameworkElement frameworkElement)
        {
            return (string)frameworkElement.GetValue(PropertyProperty);
        }

        public static void SetProperty(FrameworkElement frameworkElement, string propertyName)
        {
            frameworkElement.SetValue(PropertyProperty, propertyName);
        }

        public static readonly DependencyProperty BindProperty = DependencyProperty.RegisterAttached("Bind", typeof(string), typeof(ValidationsHelper), new PropertyMetadata(new PropertyChangedCallback(OnBindPropertyChanged)));

        public static string GetBind(FrameworkElement frameworkElement)
        {
            return (string)frameworkElement.GetValue(BindProperty);
        }

        public static void SetBind(FrameworkElement frameworkElement, string propertyName)
        {
            frameworkElement.SetValue(BindProperty, propertyName);
        }

        public static void OnBindPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs de)
        {
            if (BaseViewModel.IsDesignTime)
                return;
            DependencyObject parent = d;
            while (parent != null)
            {
                var propertyName = GetProperty((FrameworkElement)parent);
                if (!string.IsNullOrEmpty(propertyName))
                {
                    var property = ((FrameworkElement)parent).GetType().GetProperty(propertyName);

                    var descriptor = DependencyPropertyDescriptor.FromName(propertyName, parent.GetType(), parent.GetType());
                    var binding = System.Windows.Data.BindingOperations.GetBinding(((FrameworkElement)parent), descriptor.DependencyProperty);

                    var element = (FrameworkElement)d;
                    var viewModel = ((FrameworkElement)parent).DataContext as IValidationPropertyChanged;
                    var path = binding.Path.Path;
                    var data = viewModel.MessagesByProperty[path].ViewModel = new ValidationMessageViewModel() { CurrentViewModel = viewModel, PropertyName = path };
                    var propertyBindName = de.NewValue.ToString();
                    var propertyBind = element.GetType().GetProperty(propertyBindName);
                    propertyBind.SetValue(element, data, null);
                    break;
                }
                parent = VisualTreeHelper.GetParent(parent);
            }
        }

        public static void OnPropertyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs de)
        {
            if (BaseViewModel.IsDesignTime)
                return;
            var name = de.NewValue.ToString();
            //var elementProperty = d.GetType().GetProperty(name);

            var descriptor = DependencyPropertyDescriptor.FromName(name, d.GetType(), d.GetType());

            var action = new EventHandler((s, e) =>
            {
                var element = (FrameworkElement)s;
                var propertyChanged = element.DataContext as IValidationPropertyChanged;
                if (propertyChanged == null)
                    throw new Exception($"your model have to inheritance IValidationPropertyChanged [model type]: [{s.GetType()}] [binding type]: [{de.Property.PropertyType}]");
                else if (propertyChanged.AllMessages == null)
                    throw new Exception($"your model AllMessages propety cannot be null [model type]: [{s.GetType()}] [binding type]: [{de.Property.PropertyType}]");
                else if (propertyChanged.MessagesByProperty == null)
                    throw new Exception($"your model MessagesByProperty propety cannot be null [model type]: [{s.GetType()}] [binding type]: [{de.Property.PropertyType}]");
                var binding = System.Windows.Data.BindingOperations.GetBindingExpression(((FrameworkElement)d), descriptor.DependencyProperty);
                if (binding != null)
                    binding.UpdateSource();
                bool hasError = false;
                propertyChanged.AllMessages.Clear();
                foreach (var item in propertyChanged.MessagesByProperty)
                {
                    item.Value.Items.Clear();
                }

                foreach (var property in propertyChanged.GetType().GetProperties())
                {
                    var attributes = property.GetCustomAttributes(typeof(ValidationAttribute), true);
                    if (attributes.Length == 0)
                        continue;
                    else
                    {
                        if (propertyChanged.MessagesByProperty.ContainsKey(property.Name))
                            propertyChanged.MessagesByProperty[property.Name].Items.Clear();
                        foreach (ValidationAttribute attrib in attributes)
                        {
                            var valueOfProperty = property.GetValue(propertyChanged, null);
                            var error = attrib.GetMessage(valueOfProperty);
                            if (error != null)
                            {
                                hasError = true;
                                propertyChanged.AllMessages.Add(error);
                                if (!propertyChanged.MessagesByProperty.ContainsKey(property.Name))
                                {
                                    propertyChanged.MessagesByProperty[property.Name] = new ViewModelItemsInfo
                                    {
                                        Items = new ObservableCollection<ValidationMessageInfo>()
                                    };
                                }
                                propertyChanged.MessagesByProperty[property.Name].Items.Add(error);
                                break;
                            }
                        }
                        propertyChanged.MessagesByProperty[property.Name].ViewModel?.Validate();

                    }
                }
                propertyChanged.HasError = hasError;
                propertyChanged.OnPropertyChanged(nameof(propertyChanged.FirstMessage));
            });

            action(d, null);
            descriptor.AddValueChanged(d, action);
            //if (d is FrameworkElement el)
            //    el.Unloaded += (sender, args) =>
            //    {
            //        descriptor.RemoveValueChanged(sender, action);
            //    };
        }
    }
}
