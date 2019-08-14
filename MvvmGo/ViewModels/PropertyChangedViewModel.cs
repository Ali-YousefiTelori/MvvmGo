using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace MvvmGo.ViewModels
{
    /// <summary>
    /// property changed helper view model
    /// </summary>
    public abstract class PropertyChangedViewModel : INotifyPropertyChanged
    {
        Dictionary<string, List<Action>> Callbacks { get; set; } = new Dictionary<string, List<Action>>();
        /// <summary>
        /// event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// fire property changed
        /// </summary>
        /// <param name="name"></param>
        public virtual void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            if (Callbacks.TryGetValue(name, out List<Action> callbacks))
            {
                foreach (var item in callbacks.ToArray())
                {
                    item();
                }
            }
        }

        public void OnPropertyChanged(string name, Action callback)
        {
            if (Callbacks.TryGetValue(name, out List<Action> callbacks))
            {
                if (!callbacks.Contains(callback))
                    callbacks.Add(callback);
            }
            else
            {
                callbacks = new List<Action>() { callback };
                Callbacks[name] = callbacks;
            }
        }
    }
}
