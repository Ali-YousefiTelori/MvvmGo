using System.ComponentModel;

namespace MvvmGo.ViewModels
{
    /// <summary>
    /// property changed helper view model
    /// </summary>
    public abstract class PropertyChangedViewModel : INotifyPropertyChanged
    {
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
        }
    }
}
