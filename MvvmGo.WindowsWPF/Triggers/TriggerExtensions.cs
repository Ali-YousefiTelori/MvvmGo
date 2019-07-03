using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Input;

namespace MvvmGo.Triggers
{
    internal class TreeNotifyPropertyChanged
    {
        public Dictionary<string, List<TriggerItemData>> Properties { get; set; } = new Dictionary<string, List<TriggerItemData>>();

        public object Self { get; set; }
        public List<TreeNotifyPropertyChanged> Children { get; set; } = new List<TreeNotifyPropertyChanged>();
        public TreeNotifyPropertyChanged Parent { get; set; }
        public Action<object, string> ChangedAction { get; set; }
    }

    internal class TriggerItemData
    {
        public TriggerBaseInfo Trigger { get; set; }
        public System.Windows.FrameworkElement Element { get; set; }
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
                    {
                        void DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
                        {
                            element.DataContextChanged -= DataContextChanged;
                            GenerateConditions(dataConditionInfoes, element, item);
                        }

                        element.DataContextChanged += DataContextChanged;
                        continue;
                    }
                    GenerateConditions(dataConditionInfoes, element, item);
                }
                if (item is EventTriggerInfo eventTriggerInfo)
                {
#if (!NET35 && !NET40)
                    List<ConditionInfoBase> dataConditionInfoes = new List<ConditionInfoBase>();
                    SetEvent(element, element.GetType().GetEvent(eventTriggerInfo.EventName), new EventSetterInfo()
                    {
                        Command = new MvvmGo.Commands.EventCommand(() =>
                        {
                            if (eventTriggerInfo.Condition(element))
                            {
                                foreach (var setter in eventTriggerInfo.Setters)
                                {
                                    setter.SetValue(element);
                                }
                            }
                        })
                    });
                    
                    dataConditionInfoes.AddRange(eventTriggerInfo.Conditions);

                    GenerateConditions(dataConditionInfoes, element, item);
#endif
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
                    {
                        void DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
                        {
                            element.DataContextChanged -= DataContextChanged;
                            GenerateConditions(dataConditionInfoes, element, item);
                        }

                        element.DataContextChanged += DataContextChanged;
                        continue;
                    }
                    GenerateConditions(dataConditionInfoes, element, item);
                }
            }
        }

        static void GenerateConditions(IEnumerable<ConditionInfoBase> conditionInfoes, System.Windows.FrameworkElement element, TriggerBaseInfo triggerBaseInfo)
        {
            List<TriggerItemData> triggerItemDatas = new List<TriggerItemData>();
            foreach (var condition in conditionInfoes)
            {
                if (condition is DataConditionInfo dataCondition)
                {
                    var binding = (System.Windows.Data.Binding)dataCondition.Binding;
                    if (binding == null)
                        continue;
                    if (triggerBaseInfo.Conditions.Count == 0)
                        triggerBaseInfo.Conditions.Add(condition);
                    var item = new TriggerItemData() { Element = element, Trigger = triggerBaseInfo };
                    triggerItemDatas.Add(item);
                    GenerateTreeNotifyPropertyChanged(binding, element, item);
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
            DoPropertyChanged(triggerItemDatas, true, null);
        }

        static void FristTimeFillData(System.Windows.FrameworkElement element, TriggerBaseInfo triggerBaseInfo, INotifyPropertyChanged propertyChanged)
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
                    if (target != null)
                    {
                        var property = target.GetType().GetProperty(setterInfo.Property);
                        if (property != null)
                        {
                            if (setterInfo.DefaultValue == null)
                                setterInfo.DefaultValue = property.GetValue(target, null);
                        }
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

            foreach (var condition in triggerBaseInfo.Conditions)
            {
                if (condition is DataConditionInfo dataCondition)
                {
                    dataCondition.PropertyChanged = FindBinding((System.Windows.Data.Binding)dataCondition.Binding, element, propertyChanged);
                }
            }
        }

        static INotifyPropertyChanged FindBinding(System.Windows.Data.Binding binding, System.Windows.FrameworkElement element, INotifyPropertyChanged propertyChanged)
        {
            if (binding.Source != null)
                return (INotifyPropertyChanged)binding.Source;
            var firstPath = binding.Path.Path.Split('.').LastOrDefault();
            if (propertyChanged != null && propertyChanged.GetType().GetProperty(firstPath) != null)
                return propertyChanged;
            else if (element.DataContext != null && element.DataContext.GetType().GetProperty(firstPath) != null)
                return (INotifyPropertyChanged)element.DataContext;

            INotifyPropertyChanged result = propertyChanged;
            if (result == null)
                result = (INotifyPropertyChanged)element.DataContext;
            foreach (var property in binding.Path.Path.Split('.'))
            {
                if (result == null)
                    break;
                if (result.GetType().GetProperty(firstPath) != null)
                    return result;
                if (result.GetType().GetProperty(property) != null)
                    result = (INotifyPropertyChanged)result.GetType().GetProperty(property).GetValue(result, null);
            }
            return null;
        }

        internal static object GetValueBinding(System.Windows.Data.Binding binding, System.Windows.FrameworkElement element)
        {
            var allPathes = binding.Path.Path.Split('.');
            var context = element.DataContext;
            if (binding.Source != null)
                context = binding.Source as INotifyPropertyChanged;
            if (context == null)
                return null;
            INotifyPropertyChanged propertyChanged = context as INotifyPropertyChanged;

            string propertyName = "";
            foreach (var path in allPathes)
            {
                propertyName = path;
                var result = context.GetType().GetProperty(path).GetValue(context, null);
                if (result == null)
                {
                    return null;
                }
                else
                {
                    context = result;
                }
            }
            return context;
        }

        static ConcurrentDictionary<object, TreeNotifyPropertyChanged> BindedTreeNotifyProperties = new ConcurrentDictionary<object, TreeNotifyPropertyChanged>();

        static void AddNewChangedDataBinding(TreeNotifyPropertyChanged treeNotifyPropertyChanged)
        {
            if (treeNotifyPropertyChanged.Self == null)
                return;
            if (BindedTreeNotifyProperties.TryGetValue(treeNotifyPropertyChanged.Self, out TreeNotifyPropertyChanged findTreeNotifyPropertyChanged))
            {
                foreach (var item in treeNotifyPropertyChanged.Properties)
                {
                    if (findTreeNotifyPropertyChanged.Properties.TryGetValue(item.Key, out List<TriggerItemData> list))
                    {
                        list.AddRange(item.Value);
                    }
                    else
                    {
                        findTreeNotifyPropertyChanged.Properties.Add(item.Key, item.Value);
                    }
                }
            }
            else
            {
                BindedTreeNotifyProperties.TryAdd(treeNotifyPropertyChanged.Self, treeNotifyPropertyChanged);

                INotifyPropertyChanged propertyChanged = treeNotifyPropertyChanged.Self as INotifyPropertyChanged;
                if (propertyChanged != null)
                    propertyChanged.PropertyChanged += Self_PropertyChanged;
            }
        }

        static void RemoveChangedDataBinding(TreeNotifyPropertyChanged treeNotifyPropertyChanged)
        {
            if (treeNotifyPropertyChanged.Self == null)
                return;
            BindedTreeNotifyProperties.TryRemove(treeNotifyPropertyChanged.Self, out TreeNotifyPropertyChanged removed);
            if (removed == null || removed.Self == null)
                return;
            INotifyPropertyChanged propertyChanged = removed.Self as INotifyPropertyChanged;
            if (propertyChanged != null)
                propertyChanged.PropertyChanged -= Self_PropertyChanged;
        }

        private static void Self_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentPerson")
            {

            }
            if (BindedTreeNotifyProperties.TryGetValue(sender, out TreeNotifyPropertyChanged treeNotifyPropertyChanged) && treeNotifyPropertyChanged.Properties.ContainsKey(e.PropertyName))
            {
                foreach (var child in treeNotifyPropertyChanged.Children)
                {
                    bool isFirstTime = false;
                    var result = sender.GetType().GetProperty(e.PropertyName).GetValue(sender, null);
                    if (child.Self == null || (EqualTypes(result, child.Self) && result != child.Self))
                    {
                        RemoveChangedDataBinding(child);
                        isFirstTime = true;
                        child.Self = result;
                        AddNewChangedDataBinding(child);
                    }

                    foreach (var property in child.Properties)
                    {
                        DoPropertyChanged(property.Value, isFirstTime, child.Self is INotifyPropertyChanged ? (INotifyPropertyChanged)child.Self : null);
                    }
                }
                DoPropertyChanged(treeNotifyPropertyChanged.Properties[e.PropertyName], false, (INotifyPropertyChanged)sender);
            }
        }

        static bool EqualTypes(object target, object source)
        {
            if (target == null || source == null)
                return false;
            return target.GetType() == source.GetType();
        }

        static void GenerateTreeNotifyPropertyChanged(System.Windows.Data.Binding binding, System.Windows.FrameworkElement element, TriggerItemData triggerItemData)
        {
            var allPathes = binding.Path.Path.Split('.');
            var context = element.DataContext;
            if (binding.Source != null)
                context = binding.Source as INotifyPropertyChanged;
            INotifyPropertyChanged propertyChanged = context as INotifyPropertyChanged;
            var firstPath = allPathes.First();
            TreeNotifyPropertyChanged treeNotifyPropertyChanged = null;
            if (!BindedTreeNotifyProperties.TryGetValue(propertyChanged, out TreeNotifyPropertyChanged findTreeNotifyPropertyChanged))
            {
                treeNotifyPropertyChanged = new TreeNotifyPropertyChanged();
                treeNotifyPropertyChanged.Self = propertyChanged;
                treeNotifyPropertyChanged.Properties.Add(firstPath, new List<TriggerItemData>() { triggerItemData });
                AddNewChangedDataBinding(treeNotifyPropertyChanged);
                FristTimeFillData(element, triggerItemData.Trigger, propertyChanged);
            }
            else
            {
                treeNotifyPropertyChanged = findTreeNotifyPropertyChanged;
                if (findTreeNotifyPropertyChanged.Properties.ContainsKey(firstPath))
                    findTreeNotifyPropertyChanged.Properties[firstPath].Add(triggerItemData);
                else
                    findTreeNotifyPropertyChanged.Properties.Add(firstPath, new List<TriggerItemData>() { triggerItemData });
            }
            var childContext = context.GetType().GetProperty(firstPath).GetValue(context, null);
            foreach (var path in allPathes.Skip(1))
            {
                if (childContext == null)
                {
                    if (BindedTreeNotifyProperties.TryGetValue(context, out treeNotifyPropertyChanged))
                    {
                        TreeNotifyPropertyChanged parent = new TreeNotifyPropertyChanged();
                        parent.Parent = treeNotifyPropertyChanged;
                        treeNotifyPropertyChanged.Children.Add(parent);
                        parent.Properties.Add(path, new List<TriggerItemData>() { triggerItemData });

                    }
                    else
                    {
                        var result = childContext.GetType().GetProperty(path).GetValue(childContext, null);
                        if (result != null)
                        {
                            childContext = result;
                            if (!BindedTreeNotifyProperties.TryGetValue(childContext, out findTreeNotifyPropertyChanged))
                            {
                                TreeNotifyPropertyChanged parent = new TreeNotifyPropertyChanged();
                                parent.Self = result;
                                parent.Children.Add(treeNotifyPropertyChanged);
                                treeNotifyPropertyChanged.Parent = parent;
                                treeNotifyPropertyChanged = parent;
                                parent.Properties.Add(path, new List<TriggerItemData>() { triggerItemData });
                                AddNewChangedDataBinding(parent);
                            }
                            else
                            {
                                if (!treeNotifyPropertyChanged.Children.Contains(findTreeNotifyPropertyChanged))
                                    treeNotifyPropertyChanged.Children.Add(findTreeNotifyPropertyChanged);

                                treeNotifyPropertyChanged = findTreeNotifyPropertyChanged;
                                if (findTreeNotifyPropertyChanged.Properties.ContainsKey(path))
                                    findTreeNotifyPropertyChanged.Properties[path].Add(triggerItemData);
                                else
                                    findTreeNotifyPropertyChanged.Properties.Add(path, new List<TriggerItemData>() { triggerItemData });
                            }
                        }
                        else
                        {
                            if (BindedTreeNotifyProperties.TryGetValue(childContext, out treeNotifyPropertyChanged))
                            {
                                if (findTreeNotifyPropertyChanged.Properties.ContainsKey(path))
                                    findTreeNotifyPropertyChanged.Properties[path].Add(triggerItemData);
                                else
                                    findTreeNotifyPropertyChanged.Properties.Add(path, new List<TriggerItemData>() { triggerItemData });

                                TreeNotifyPropertyChanged parent = new TreeNotifyPropertyChanged();
                                parent.Parent = treeNotifyPropertyChanged;
                                treeNotifyPropertyChanged.Children.Add(parent);
                                parent.Properties.Add(path, new List<TriggerItemData>() { triggerItemData });
                            }
                            else
                            {

                            }
                        }
                    }
                }
                else
                {
                    TreeNotifyPropertyChanged parent = new TreeNotifyPropertyChanged();
                    parent.Parent = treeNotifyPropertyChanged;
                    treeNotifyPropertyChanged.Children.Add(parent);
                    parent.Properties.Add(path, new List<TriggerItemData>() { triggerItemData });

                }
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

        private static void DoPropertyChanged(List<TriggerItemData> triggers, bool isFirstTime, INotifyPropertyChanged propertyChanged)
        {
            List<TriggerItemChanged> Changes = new List<TriggerItemChanged>();
            var exist = triggers.Any(x => x.Trigger.Setters.Any(y => y is EventSetterInfo));
            if (exist)
            {
            }
            foreach (var triggerItem in triggers)
            {
                if (isFirstTime)
                    FristTimeFillData(triggerItem.Element, triggerItem.Trigger, propertyChanged);
                if (triggerItem.Trigger.Conditions.Any(x => x is DataConditionInfo y && y.PropertyChanged == null))
                {
                    FristTimeFillData(triggerItem.Element, triggerItem.Trigger, propertyChanged);
                }

                if (triggerItem.Trigger.Condition(triggerItem.Element))
                {
                    foreach (var setter in triggerItem.Trigger.Setters)
                    {
                        if (setter is SetterInfo setterInfo)
                        {
                            var target = triggerItem.Element;
                            setterInfo.SetValue(triggerItem.Element);
                            if (setterInfo.ElementName != null)
                            {
                                target = (System.Windows.FrameworkElement)target.FindName(setterInfo.ElementName);
                            }
                            var find = Changes.FirstOrDefault(x => x.Name == setterInfo.Property && x.Target == target);
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

                            //var find = Changes.FirstOrDefault(x => x.Name == eventSetterInfo.EventName && x.Target == target);
                            //if (find != null)
                            //    find.HasChange = true;
                            //else
                            Changes.Add(new TriggerItemChanged() { HasChange = true, Target = target, Name = eventSetterInfo.EventName, SetterInfoBase = setter });
                        }
                    }
                }
                else
                {
                    foreach (var setter in triggerItem.Trigger.Setters)
                    {
                        if (setter is SetterInfo setterInfo)
                        {
                            var target = triggerItem.Element;
                            if (setterInfo.ElementName != null)
                            {
                                target = (System.Windows.FrameworkElement)target.FindName(setterInfo.ElementName);
                            }
                            if (target != null)
                            {
                                var property = target.GetType().GetProperty(setterInfo.Property);
                                if (property != null)
                                {
                                    var find = Changes.FirstOrDefault(x => x.Name == setterInfo.Property && x.Target == target);// && x.SetterInfoBase == setter
                                    if (find == null)
                                        Changes.Add(new TriggerItemChanged() { HasChange = false, Target = target, DefaultValue = setterInfo.DefaultValue, Name = setterInfo.Property, SetterInfoBase = setter });
                                }
                            }
                        }
                        else if (setter is EventSetterInfo eventSetterInfo)
                        {
                            var target = triggerItem.Element;
                            if (eventSetterInfo.ElementName != null)
                            {
                                target = (System.Windows.FrameworkElement)target.FindName(eventSetterInfo.ElementName);
                            }
                            //var find = Changes.FirstOrDefault(x => x.Name == eventSetterInfo.EventName && x.Target == target);// && x.SetterInfoBase == setter
                            Changes.Add(new TriggerItemChanged() { HasChange = false, Target = target, Name = eventSetterInfo.EventName, SetterInfoBase = setter });
                        }
                    }
                }
            }

            var exist2 = Changes.Any(x => x.SetterInfoBase is EventSetterInfo);
            foreach (var item in Changes)
            {
                if (!item.HasChange)
                {
                    if (item.SetterInfoBase is EventSetterInfo)
                    {
                    }
                    item.SetterInfoBase.SetCustomValue(item.Target, item.DefaultValue);
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

            EventHandlerInfo eventHandlerInfo = new EventHandlerInfo() { EventCommand = eventSetterInfo.Command, Target = target, EventSetterInfo = eventSetterInfo, CommandParameter = parameterValue };
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
            var find = GetEventHandler(target, eventInfo.Name);
        }
        /// <summary>
        /// Gets the EventHandler delegate attached to the specified event and object
        /// </summary>
        /// <param name="obj">object that contains the event</param>
        /// <param name="eventName">name of the event, e.g. "Click"</param>
        public static Delegate GetEventHandler(object obj, string eventName)
        {
            Delegate retDelegate = null;
            FieldInfo fi = obj.GetType().GetField("Event" + eventName,
                                                   BindingFlags.NonPublic |
                                                   BindingFlags.Static |
                                                   BindingFlags.Instance |
                                                   BindingFlags.FlattenHierarchy |
                                                   BindingFlags.IgnoreCase);
            if (fi != null)
            {
                object value = fi.GetValue(obj);
                if (value is Delegate)
                    retDelegate = (Delegate)value;
                else if (value != null) // value may be just object
                {
                    PropertyInfo pi = obj.GetType().GetProperty("Events",
                                                   BindingFlags.NonPublic |
                                                   BindingFlags.Instance);
                    if (pi != null)
                    {
                        EventHandlerList eventHandlers = pi.GetValue(obj, null) as EventHandlerList;
                        if (eventHandlers != null)
                        {
                            retDelegate = eventHandlers[value];
                        }
                    }
                }
            }
            return retDelegate;
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
