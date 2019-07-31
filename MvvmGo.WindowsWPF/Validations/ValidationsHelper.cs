using MvvmGo.Attributes;
using MvvmGo.Models;
using MvvmGo.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xaml;

namespace MvvmGo.Validations
{


    //public class ErrorMessage : MarkupExtension
    //{
    //    public static readonly DependencyProperty ElementProperty = DependencyProperty.RegisterAttached("Element", typeof(UIElement), typeof(ErrorMessage), new PropertyMetadata(new PropertyChangedCallback(OnPropertyPropertyChanged)));

    //    private static void OnPropertyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //    {
    //    }

    //    public static UIElement GetElement(FrameworkElement frameworkElement)
    //    {
    //        return (UIElement)frameworkElement.GetValue(ElementProperty);
    //    }

    //    public static void SetElement(FrameworkElement frameworkElement, UIElement propertyName)
    //    {
    //        frameworkElement.SetValue(ElementProperty, propertyName);
    //    }
    //    public ErrorMessage(string methodName)
    //    {
    //    }

    //    public override object ProvideValue(IServiceProvider serviceProvider)
    //    {
    //        IRootObjectProvider rootProvider = serviceProvider.GetService(typeof(IRootObjectProvider)) as IRootObjectProvider;
    //        IProvideValueTarget target = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
    //        var provideValueTarget = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));
    //        var bindingExpression = BindingOperations.GetBindingExpression((DependencyObject)provideValueTarget.TargetObject, (DependencyProperty)provideValueTarget.TargetProperty);
    //        var source = bindingExpression.DataItem;
    //        return "ok ok";
    //        //return bin.ProvideValue(serviceProvider);
    //    }
    //}

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
                    if (descriptor == null)
                        break;
                    var binding = System.Windows.Data.BindingOperations.GetBindingExpression(((FrameworkElement)parent), descriptor.DependencyProperty);
                    if (binding == null)
                        break;

                    var element = (FrameworkElement)d;
                    var viewModel = ((FrameworkElement)parent).DataContext as IValidationPropertyChanged;
                    if (viewModel == null)
                        return;
                    var objectInstance = GetContext(binding, out string path);
                    if (objectInstance == null)
                        return;
                    var typeOfData = objectInstance.GetType();
                    string fullNameOfProperty = typeOfData.FullName + "." + path;

                    if (!viewModel.MessagesByProperty.ContainsKey(fullNameOfProperty))
                    {
                        var myValidations = viewModel.MessagesByProperty[fullNameOfProperty] = new ViewModelItemsInfo
                        {
                            Items = new ObservableCollection<ValidationMessageInfo>()
                        };

                    }

                    var data = viewModel.MessagesByProperty[fullNameOfProperty].ViewModel = new ValidationMessageViewModel() { CurrentViewModel = viewModel, PropertyName = fullNameOfProperty };
                    var propertyBindName = de.NewValue.ToString();
                    var propertyBind = element.GetType().GetProperty(propertyBindName);
                    propertyBind.SetValue(element, data, null);

                    return;
                }
                parent = VisualTreeHelper.GetParent(parent);
            }

            var emptyElement = (FrameworkElement)d;
            var emptyPropertyBindName = de.NewValue.ToString();
            var emptyPpropertyBind = emptyElement.GetType().GetProperty(emptyPropertyBindName);
            emptyPpropertyBind.SetValue(emptyElement, null, null);
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
                    return;
                // throw new Exception($"your model have to inheritance IValidationPropertyChanged [model type]: [{s.GetType()}] [binding type]: [{de.Property.PropertyType}]");
                else if (propertyChanged.AllMessages == null)
                    throw new Exception($"your model AllMessages propety cannot be null [model type]: [{s.GetType()}] [binding type]: [{de.Property.PropertyType}]");
                else if (propertyChanged.MessagesByProperty == null)
                    throw new Exception($"your model MessagesByProperty propety cannot be null [model type]: [{s.GetType()}] [binding type]: [{de.Property.PropertyType}]");
                if (descriptor == null && element is PasswordBox password)
                {
                    if (password.DataContext is BaseViewModel bvm)
                    {
                        bvm.OnPropertyChanged(name);
                    }
                    return;
                }
                var binding = System.Windows.Data.BindingOperations.GetBindingExpression(((FrameworkElement)d), descriptor.DependencyProperty);
                if (binding == null)
                    return;
                var property = d.GetType().GetProperty(name);
                var objectValue = property.GetValue(d, null);
                if (descriptor.DependencyProperty.PropertyType == typeof(string) && objectValue != null && string.IsNullOrEmpty(objectValue.ToString()) && Nullable.GetUnderlyingType(property.PropertyType) != null)
                    property.SetValue(d, null, null);
                if (binding != null)
                    binding.UpdateSource();
                var bindingPropertyName = "";

                bool hasError = false;
                var mainValue = property.GetValue(d, null);
                //propertyChanged.AllMessages.Clear();

                //foreach (var item in propertyChanged.MessagesByProperty)
                //{
                //    item.Value.Items.Clear();
                //}


                // 
                var objectInstance = GetContext(binding, out bindingPropertyName);
                if (objectInstance == null)
                    return;
                var typeOfData = objectInstance.GetType();

                property = typeOfData.GetProperty(bindingPropertyName);
                if (property == null)
                    throw new Exception($"property {bindingPropertyName} not found on {objectInstance}");
                string fullNameOfProperty = typeOfData.FullName + "." + property.Name;

                if (property != null)
                {
                    if (ValidationsHelperExtensions.PropertyValidations.ContainsKey(propertyChanged) && (ValidationsHelperExtensions.PropertyValidations[propertyChanged].TryGetValue(objectInstance, out Dictionary<string,ValidationsBuilder> properties) || ValidationsHelperExtensions.PropertyValidations[propertyChanged].TryGetValue(typeOfData, out properties))
                    && ValidationsHelperExtensions.PropertyValidations[propertyChanged][typeOfData].ContainsKey(bindingPropertyName))
                    {
                        //var properties = ValidationsHelperExtensions.PropertyValidations[propertyChanged][typeOfData];
                        foreach (var propertyValidation in properties)
                        {
                            if (propertyValidation.Value.Properties.Count > 0)
                            {
                                bool myHasError = false;

                                foreach (var validateProp in propertyValidation.Value.Properties)
                                {
                                    var myFullNameOfProperty = typeOfData.FullName + "." + validateProp.Name;
                                    if (propertyChanged.MessagesByProperty.ContainsKey(myFullNameOfProperty))
                                        propertyChanged.MessagesByProperty[myFullNameOfProperty].Items.Clear();
                                    else
                                        propertyChanged.MessagesByProperty[myFullNameOfProperty] = new ViewModelItemsInfo()
                                        {
                                            Items = new ObservableCollection<ValidationMessageInfo>(),
                                            ViewModel = new ValidationMessageViewModel() { CurrentViewModel = propertyChanged, PropertyName = myFullNameOfProperty }
                                        };
                                    foreach (var attrib in validateProp.Validations)
                                    {
                                        ValidationMessageInfo error = null;
                                        if (validateProp.Name == property.Name)
                                            error = attrib.GetMessage(mainValue);
                                        else
                                        {
                                            var myproperty = objectInstance.GetType().GetProperty(validateProp.Name);
                                            var value = myproperty.GetValue(objectInstance);
                                            error = attrib.GetMessage(value);
                                        }
                                        if (error != null)
                                        {
                                            error.PropertyName = validateProp.Name;
                                            myHasError = true;
                                            if (validateProp.Name == property.Name)
                                            {
                                                propertyChanged.AllMessages.Remove(propertyChanged.AllMessages.FirstOrDefault(x => x.PropertyName == validateProp.Name));
                                                propertyChanged.AllMessages.Add(error);
                                            }
                                            propertyChanged.MessagesByProperty[myFullNameOfProperty].Items.Remove(
                                                propertyChanged.MessagesByProperty[myFullNameOfProperty].Items.FirstOrDefault(x => x.PropertyName == validateProp.Name));
                                            propertyChanged.MessagesByProperty[myFullNameOfProperty].Items.Add(error);
                                            propertyChanged.MessagesByProperty[myFullNameOfProperty].ViewModel?.Validate();

                                            break;
                                        }
                                        else
                                        {
                                            propertyChanged.AllMessages.Remove(propertyChanged.AllMessages.FirstOrDefault(x => x.PropertyName == validateProp.Name));
                                        }
                                    }
                                }
                                propertyValidation.Value.HasError = myHasError;
                                if (myHasError)
                                    hasError = true;
                            }
                        }

                    }
                    else
                    {
                        if (!propertyChanged.MessagesByProperty.ContainsKey(fullNameOfProperty))
                        {
                            propertyChanged.MessagesByProperty[fullNameOfProperty] = new ViewModelItemsInfo
                            {
                                Items = new ObservableCollection<ValidationMessageInfo>()
                            };
                        }
                        var attributes = property.GetCustomAttributes(typeof(ValidationAttribute), true);
                        if (attributes.Length > 0)
                        {
                            if (propertyChanged.MessagesByProperty.ContainsKey(fullNameOfProperty))
                                propertyChanged.MessagesByProperty[fullNameOfProperty].Items.Clear();
                            foreach (ValidationAttribute attrib in attributes)
                            {
                                //var valueOfProperty = property.GetValue(objectInstance, null);
                                var error = attrib.GetMessage(mainValue);
                                if (error != null)
                                {
                                    error.PropertyName = bindingPropertyName;
                                    hasError = true;
                                    propertyChanged.AllMessages.Remove(propertyChanged.AllMessages.FirstOrDefault(x => x.PropertyName == bindingPropertyName));
                                    propertyChanged.AllMessages.Add(error);

                                    propertyChanged.MessagesByProperty[fullNameOfProperty].Items.Remove(
                                        propertyChanged.MessagesByProperty[fullNameOfProperty].Items.FirstOrDefault(x => x.PropertyName == bindingPropertyName));
                                    propertyChanged.MessagesByProperty[fullNameOfProperty].Items.Add(error);
                                    break;
                                }
                            }
                            propertyChanged.MessagesByProperty[fullNameOfProperty].ViewModel?.Validate();
                        }
                        else
                            return;
                    }
                }
                if (!hasError)
                {
                    propertyChanged.AllMessages.Remove(propertyChanged.AllMessages.FirstOrDefault(x => x.PropertyName == bindingPropertyName));
                }
                var myErrors = propertyChanged.MessagesByProperty[fullNameOfProperty];
                binding.ParentBinding.ValidationRules.Clear();
                foreach (var item in myErrors.Items)
                {
                    binding.ParentBinding.ValidationRules.Add(new CustomValidationRule(item));
                }
                binding.UpdateSource();
                propertyChanged.HasError = propertyChanged.AllMessages.Count(x => x.Type == ValidationMessageType.Error) > 0;
                propertyChanged.OnPropertyChanged(nameof(propertyChanged.FirstMessage));
            });

            ((FrameworkElement)d).Loaded += (s, e) =>
            {
                action(d, null);
            };
            ValidationMessageViewModel.AllPropertyChanges.Add(() =>
            {
                action(d, null);
            });
            if (descriptor == null && d is PasswordBox passwordBox)
            {
                passwordBox.PasswordChanged += (s, e) =>
                {
                    action(s, null);
                };
            }
            else
                descriptor.AddValueChanged(d, action);
            //if (d is FrameworkElement el)
            //    el.Unloaded += (sender, args) =>
            //    {
            //        descriptor.RemoveValueChanged(sender, action);
            //    };
        }

        static object GetContext(System.Windows.Data.BindingExpression binding, out string propertyName)
        {
            var context = binding.DataItem;

            var allPathes = binding.ParentBinding.Path.Path.Split('.');

            propertyName = "";
            int i = 1;
            foreach (var path in allPathes)
            {
                propertyName = path;
                if (i != allPathes.Length)
                {
                    if (context != null)
                    {
                        context = context.GetType().GetProperty(path).GetValue(context, null);
                    }
                }
                i++;
            }
            return context;
        }

    }
}
