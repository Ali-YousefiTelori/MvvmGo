using MvvmGo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MvvmGo.Attributes
{
    public abstract class ValidationAttribute : Attribute
    {
        public abstract ValidationMessageInfo GetMessage(object value);
    }
}
