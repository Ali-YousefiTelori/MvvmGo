using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MvvmGo.Extensions
{
    public static class ViewModelExtensions
    {
        public static T GetBaseViewModel<T>(this object binding) where T : class
        {
            if (binding == null)
                return null;
            return (T)binding;
        }

        public static T SetPropertyChanged<T>(this object binding) where T : class
        {
            if (binding == null)
                return null;
            return (T)binding;
        }
    }
}
