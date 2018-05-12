using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Input;

namespace MvvmGo.Triggers
{
    internal class TriggerItemData
    {
        public TriggerBaseInfo Trigger { get; set; }
        public System.Windows.FrameworkElement Element { get; set; }
        public IEnumerable<ConditionInfoBase> ConditionInfoes { get; set; }
        public SetterCollectionInfo Setters { get; set; }
    }

    public class TriggerExtensions
    {

        public static readonly System.Windows.DependencyProperty TriggersProperty = System.Windows.DependencyProperty.RegisterAttached(
           "Triggers",
           typeof(TriggerCollections),
           typeof(TriggerExtensions), new System.Windows.PropertyMetadata(new System.Windows.PropertyChangedCallback(OnTriggersPropertyChanged)));


        public static TriggerCollections GetTriggers(System.Windows.FrameworkElement frameworkElement)
        {
            return (TriggerCollections)frameworkElement.GetValue(TriggersProperty);
        }

        public static void SetTriggers(System.Windows.FrameworkElement frameworkElement, TriggerCollections triggers)
        {
            frameworkElement.SetValue(TriggersProperty, triggers);
        }

        //static Dictionary<INotifyPropertyChanged, List<string>> properties = new Dictionary<INotifyPropertyChanged, List<string>>();
        static Dictionary<string, List<TriggerItemData>> BindedProperties { get; set; } = new Dictionary<string, List<TriggerItemData>>();
        static List<INotifyPropertyChanged> PropertyChangeds = new List<INotifyPropertyChanged>();
        public static void OnTriggersPropertyChanged(System.Windows.DependencyObject d, System.Windows.DependencyPropertyChangedEventArgs de)
        {
            var element = (System.Windows.FrameworkElement)d;
            TriggerCollections items = (TriggerCollections)element.GetValue(TriggersProperty);
            if (items == null && de.OldValue != null)
            {
                foreach (var item in (TriggerCollections)de.OldValue)
                {

                }
                return;
            }
            foreach (var item in items)
            {
                if (item is DataTriggerInfo dataTrigger)
                {
                    List<ConditionInfoBase> dataConditionInfoes = new List<ConditionInfoBase>();
                    if (dataTrigger.Binding != null)
                        dataConditionInfoes.Add(new DataConditionInfo() { Binding = (System.Windows.Data.Binding)dataTrigger.Binding, IsInvert = dataTrigger.IsInvert, Value = dataTrigger.Value });
                    dataConditionInfoes.AddRange(dataTrigger.Conditions);
                    if (element.DataContext == null)
                        continue;
                    GenerateConditions(dataConditionInfoes, element, item);
                }
                else if (item is TriggerInfo triggerInfo)
                {
                    List<ConditionInfoBase> dataConditionInfoes = new List<ConditionInfoBase>();
                    if (triggerInfo.TargetName != null)
                    {
                        dataConditionInfoes.Add(new ConditionInfo()
                        {
                            ElementName = triggerInfo.TargetName,
                            IsInvert = triggerInfo.IsInvert,
                            Value = triggerInfo.Value,
                            Property = triggerInfo.Property
                        });
                    }
                    else if (triggerInfo.Property != null)
                    {
                        dataConditionInfoes.Add(new ConditionInfo()
                        {
                            Element = element,
                            IsInvert = triggerInfo.IsInvert,
                            Value = triggerInfo.Value,
                            Property = triggerInfo.Property
                        });
                    }
                    else if (triggerInfo.TargetElement != null)
                    {
                        dataConditionInfoes.Add(new ConditionInfo()
                        {
                            Element = triggerInfo.TargetElement,
                            IsInvert = triggerInfo.IsInvert,
                            Value = triggerInfo.Value,
                            Property = triggerInfo.Property
                        });
                    }
                    dataConditionInfoes.AddRange(triggerInfo.Conditions);
                    if (element.DataContext == null)
                        continue;
                    GenerateConditions(dataConditionInfoes, element, item);
                }
            }
        }

        static void GenerateConditions(IEnumerable<ConditionInfoBase> conditionInfoes, System.Windows.FrameworkElement element, TriggerBaseInfo triggerBaseInfo)
        {
            foreach (var condition in conditionInfoes)
            {
                if (condition is DataConditionInfo dataCondition)
                {
                    var binding = (System.Windows.Data.Binding)dataCondition.Binding;
                    if (binding == null)
                        continue;
                    if (triggerBaseInfo.Conditions.Count == 0)
                        triggerBaseInfo.Conditions.Add(condition);
                    WhenPropertyChanged(binding, element, (propertyChanged, propertyName) =>
                    {
                        if (propertyChanged != null)
                        {
                            foreach (var setter in triggerBaseInfo.Setters)
                            {
                                //get defualt values
                                if (setter is SetterInfo setterInfo)
                                {
                                    var target = element;
                                    if (setterInfo.ElementName != null)
                                    {
                                        target = (System.Windows.FrameworkElement)target.FindName(setterInfo.ElementName);
                                    }
                                    if (target == null)
                                        throw new Exception($"target by name {setterInfo.ElementName} not found!");
                                    var property = target.GetType().GetProperty(setterInfo.Property);
                                    if (property != null)
                                    {
                                        if (setterInfo.DefaultValue == null)
                                            setterInfo.DefaultValue = property.GetValue(target, null);
                                    }
                                }
                                else if (setter is DataSetterInfo dataSetterInfo)
                                {
                                    //var target = element;
                                    //if (setterInfo.ElementName != null)
                                    //{
                                    //    target = (System.Windows.FrameworkElement)target.FindName(setterInfo.ElementName);
                                    //}
                                    //if (target == null)
                                    //    throw new Exception($"target by name {setterInfo.ElementName} not found!");
                                    //var property = target.GetType().GetProperty(setterInfo.Property);
                                    //if (property != null)
                                    //{
                                    //    if (setterInfo.DefaultValue == null)
                                    //        setterInfo.DefaultValue = property.GetValue(target, null);
                                    //}
                                }
                            }
                            dataCondition.PropertyChanged = propertyChanged;
                            if (!BindedProperties.ContainsKey(propertyName))
                            {
                                BindedProperties[propertyName] = new List<TriggerItemData>() { new TriggerItemData() { Element = element, Trigger = triggerBaseInfo, ConditionInfoes = conditionInfoes, Setters = triggerBaseInfo.Setters } };
                            }
                            else if (!BindedProperties[propertyName].Any(x => x.Element == element && x.Trigger == triggerBaseInfo))
                            {
                                BindedProperties[propertyName].Add(new TriggerItemData() { Element = element, Trigger = triggerBaseInfo, ConditionInfoes = conditionInfoes, Setters = triggerBaseInfo.Setters });
                            }
                            if (!PropertyChangeds.Contains(propertyChanged))
                            {
                                PropertyChangeds.Add(propertyChanged);
                                propertyChanged.PropertyChanged += PropertyChanged_PropertyChanged;
                            }
                            if (!conditionInfoes.Where(x => x is DataConditionInfo).Select(x => (DataConditionInfo)x).Any(x => x.PropertyChanged == null))
                                PropertyChanged_PropertyChanged(propertyChanged, new PropertyChangedEventArgs(propertyName));
                        }
                    });
                }
                else if (condition is ConditionInfo conditionInfo)
                {
                    if (triggerBaseInfo.Conditions.Count == 0)
                        triggerBaseInfo.Conditions.Add(condition);
                    var target = element;
                    if (conditionInfo.ElementName != null)
                    {
                        target = (System.Windows.FrameworkElement)target.FindName(conditionInfo.ElementName);
                    }
                    if (target == null)
                        throw new Exception($"target by name {conditionInfo.ElementName} not found!");
                    var property = target.GetType().GetProperty(conditionInfo.Property);
                    if (property == null)
                        throw new Exception($"property {conditionInfo.Property} not found by target name {conditionInfo.ElementName}!");

                    var descriptor = DependencyPropertyDescriptor.FromName(property.Name, target.GetType(), target.GetType());
                    if (descriptor == null)
                        throw new Exception($"Descriptor not found by property name {property.Name}");
                    var action = new EventHandler((s, e) =>
                    {
                        if (triggerBaseInfo.Condition(element))
                        {
                            foreach (var setter in triggerBaseInfo.Setters)
                            {
                                setter.SetValue(element);
                            }
                        }
                    });

                    (target).Loaded += (s, e) =>
                    {
                        action(target, null);
                    };
                    descriptor.AddValueChanged(target, action);
                }
            }
        }

        static object GetValueBinding(System.Windows.Data.Binding binding, System.Windows.FrameworkElement element)
        {
            var allPathes = binding.Path.Path.Split('.');
            var context = element.DataContext;
            if (binding.Source != null)
                context = binding.Source as INotifyPropertyChanged;
            INotifyPropertyChanged propertyChanged = context as INotifyPropertyChanged;

            string propertyName = "";
            int i = 1;
            foreach (var path in allPathes)
            {
                propertyName = path;
                if (i == allPathes.Length)
                {
                    propertyChanged = context as INotifyPropertyChanged;
                }
                else
                {
                    var result = context.GetType().GetProperty(path).GetValue(context, null);
                    if (result == null)
                    {
                        propertyChanged = null;
                        break;
                    }
                    else
                    {
                        context = result;
                    }
                }
                i++;
            }
            return context;
        }

        static void WhenPropertyChanged(System.Windows.Data.Binding binding, System.Windows.FrameworkElement element, Action<INotifyPropertyChanged, string> Changed)
        {
            var allPathes = binding.Path.Path.Split('.');
            var context = element.DataContext;
            if (binding.Source != null)
                context = binding.Source as INotifyPropertyChanged;
            INotifyPropertyChanged propertyChanged = context as INotifyPropertyChanged;

            string propertyName = "";
            int i = 1;
            foreach (var path in allPathes)
            {
                propertyName = path;
                if (i == allPathes.Length)
                {
                    propertyChanged = context as INotifyPropertyChanged;
                }
                else
                {
                    var result = context.GetType().GetProperty(path).GetValue(context, null);
                    if (result == null)
                    {
                        var changed = context as INotifyPropertyChanged;
                        changed.PropertyChanged += (s, e) =>
                        {
                            if (e.PropertyName == propertyName)
                                WhenPropertyChanged(binding, element, Changed);
                        };
                        propertyChanged = null;
                        break;
                    }
                    else
                    {
                        context = result;
                    }
                }
                i++;
            }
            if (propertyChanged != null)
            {
                Changed(propertyChanged, propertyName);
            }
        }

        private class TriggerItemChanged
        {
            public string Name { get; set; }
            public bool HasChange { get; set; }
            public object Target { get; set; }
            public object DefaultValue { get; set; }
            public SetterInfoBase SetterInfoBase { get; set; }
        }

        private static void PropertyChanged_PropertyChanged(object sender, PropertyChangedEventArgs arg)
        {
            if (BindedProperties.ContainsKey(arg.PropertyName))
            {
                List<TriggerItemChanged> Changes = new List<TriggerItemChanged>();
                foreach (var triggerItem in BindedProperties[arg.PropertyName])//.Where(x => x.ConditionInfoes.Any(y => y is DataConditionInfo && ((DataConditionInfo)y).PropertyChanged == sender))
                {
                    if (triggerItem.Trigger.Condition(triggerItem.Element))
                    {
                        foreach (var setter in triggerItem.Setters)
                        {
                            //setter.SetValue()
                            //var target = triggerItem.Element;
                            //if (setter.ElementName != null)
                            //{
                            //    target = (System.Windows.FrameworkElement)target.FindName(setter.ElementName);
                            //}
                            //if (target == null)
                            //    continue;
                            if (setter is SetterInfo setterInfo)
                            {
                                var target = triggerItem.Element;
                                setterInfo.SetValue(triggerItem.Element);
                                if (setterInfo.ElementName != null)
                                {
                                    target = (System.Windows.FrameworkElement)target.FindName(setterInfo.ElementName);
                                }
                                var find = Changes.FirstOrDefault(x => x.Name == setterInfo.Property && x.Target == target );//&& x.SetterInfoBase == setter
                                if (find != null)
                                    find.HasChange = true;
                                else
                                    Changes.Add(new TriggerItemChanged() { HasChange = true, Target = target, DefaultValue = setterInfo.DefaultValue, Name = setterInfo.Property, SetterInfoBase = setter });
                            }
                            else if (setter is EventSetterInfo eventSetterInfo)
                            {
                                var target = triggerItem.Element;
                                eventSetterInfo.SetValue(triggerItem.Element);
                                if (eventSetterInfo.ElementName != null)
                                {
                                    target = (System.Windows.FrameworkElement)target.FindName(eventSetterInfo.ElementName);
                                }
                                //var eventInfo = target.GetType().GetEvent(eventSetterInfo.EventName);
                                //if (eventInfo != null)
                                //{
                                //    RemoveEvent(target, eventInfo, eventSetterInfo);
                                //    SetEvent(target, eventInfo, eventSetterInfo);
                                //}
                                var find = Changes.FirstOrDefault(x => x.Name == eventSetterInfo.EventName && x.Target == target );//&& x.SetterInfoBase == setter
                                if (find != null)
                                    find.HasChange = true;
                                else
                                    Changes.Add(new TriggerItemChanged() { HasChange = true, Target = target, Name = eventSetterInfo.EventName, SetterInfoBase = setter });
                            }
                        }
                    }
                    else
                    {
                        foreach (var setter in triggerItem.Setters)
                        {
                            if (setter is SetterInfo setterInfo)
                            {
                                var target = triggerItem.Element;
                                if (setterInfo.ElementName != null)
                                {
                                    target = (System.Windows.FrameworkElement)target.FindName(setterInfo.ElementName);
                                }

                                var property = target.GetType().GetProperty(setterInfo.Property);
                                if (property != null)
                                {
                                    var find = Changes.FirstOrDefault(x => x.Name == setterInfo.Property && x.Target == target && x.SetterInfoBase == setter);
                                    if (find == null)
                                        Changes.Add(new TriggerItemChanged() { HasChange = false, Target = target, DefaultValue = setterInfo.DefaultValue, Name = setterInfo.Property, SetterInfoBase = setter });
                                }
                            }
                            else if (setter is EventSetterInfo eventSetterInfo)
                            {
                                var target = triggerItem.Element;
                                if (eventSetterInfo.ElementName != null)
                                {
                                    target = (System.Windows.FrameworkElement)target.FindName(eventSetterInfo.ElementName);
                                }
                                var find = Changes.FirstOrDefault(x => x.Name == eventSetterInfo.EventName && x.Target == target && x.SetterInfoBase == setter);
                                if (find == null)
                                    Changes.Add(new TriggerItemChanged() { HasChange = false, Target = target, Name = eventSetterInfo.EventName, SetterInfoBase = setter });
                            }
                        }
                    }
                }

                foreach (var item in Changes)
                {
                    if (!item.HasChange)
                    {
                        item.SetterInfoBase.SetCustomValue(item.Target, item.DefaultValue);
                        //if (item.SetterInfoBase is SetterInfo setterInfo)
                        //{
                        //    var property = item.Target.GetType().GetProperty(item.Name);
                        //    if (property != null)
                        //    {
                        //        SetValue(item.Target, property, item.DefaultValue);
                        //    }
                        //}
                        //else if (item.SetterInfoBase is EventSetterInfo eventSetterInfo)
                        //{
                        //    var eventInfo = item.Target.GetType().GetEvent(eventSetterInfo.EventName);
                        //    if (eventInfo != null)
                        //    {
                        //        RemoveEvent(item.Target, eventInfo, eventSetterInfo);
                        //    }
                        //}
                    }
                }
            }
        }


        internal static void SetEvent(object target, EventInfo eventInfo, EventSetterInfo eventSetterInfo)
        {
            var parameterValue = eventSetterInfo.GetValue(EventSetterInfo.CommandParameterProperty);
            if (parameterValue == null)
            {
                var binding = System.Windows.Data.BindingOperations.GetBinding(eventSetterInfo, EventSetterInfo.CommandParameterProperty);
                if (binding != null)
                {
                    if (binding.Path == null)
                    {
                        parameterValue = ((System.Windows.FrameworkElement)target).DataContext;
                    }
                    else
                    {
                        parameterValue = GetValueBinding(binding, ((System.Windows.FrameworkElement)target));
                    }
                }
            }

            EventHandlerInfo eventHandlerInfo = new EventHandlerInfo() { EventCommand = eventSetterInfo.Command, CommandParameter = parameterValue };
            var parameters = eventInfo.EventHandlerType.GetMethod("Invoke").GetParameters();
            var method = eventHandlerInfo.GetType().GetMethods().FirstOrDefault(x => x.Name == "Run" && x.GetParameters().Length == parameters.Length).MakeGenericMethod(parameters.Select(x => x.ParameterType).ToArray());

            var del = Delegate.CreateDelegate(eventInfo.EventHandlerType, eventHandlerInfo, method);
            eventSetterInfo.Handler = del;
            eventInfo.AddEventHandler(target, del);
        }

        internal static void RemoveEvent(object target, EventInfo eventInfo, EventSetterInfo eventSetterInfo)
        {
            if (eventSetterInfo.Handler == null)
                return;
            eventInfo.RemoveEventHandler(target, eventSetterInfo.Handler);
        }

        internal static void FireEvent(object onMe, string invokeMe, params object[] eventParams)
        {
            MulticastDelegate eventDelagate =
                  (MulticastDelegate)onMe.GetType().GetField(invokeMe,
                   BindingFlags.Instance |
                   BindingFlags.NonPublic).GetValue(onMe);

            Delegate[] delegates = eventDelagate.GetInvocationList();
            if (delegates.Length == 0)
                return;
            List<object> AllParameters = eventParams.ToList();
            var parameters = delegates.FirstOrDefault().Method.GetParameters();
            if (parameters.Length > AllParameters.Count)
            {
                foreach (var item in parameters.Skip(AllParameters.Count))
                {
                    AllParameters.Add(GetDefault(item.ParameterType));
                }
            }
            else if (parameters.Length < AllParameters.Count)
            {
                AllParameters.RemoveRange(parameters.Length, AllParameters.Count - parameters.Length);
            }

            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].ParameterType == typeof(object))
                    continue;
                if (AllParameters[i] == null)
                {
                    AllParameters[i] = GetDefault(parameters[i].ParameterType);
                }
                else if (parameters[i].ParameterType != AllParameters[i].GetType())
                {
                    AllParameters[i] = GetDefault(parameters[i].ParameterType);
                }
            }

            foreach (Delegate dlg in delegates)
            {
                dlg.Method.Invoke(dlg.Target, AllParameters.ToArray());
            }
        }

        static object GetDefault(Type t)
        {
            Func<object> f = GetDefault<object>;
            return f.Method.GetGenericMethodDefinition().MakeGenericMethod(t).Invoke(null, null);
        }

        static T GetDefault<T>()
        {
            return default(T);
        }
    }
}
