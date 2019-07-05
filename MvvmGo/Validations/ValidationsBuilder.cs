using MvvmGo.Attributes;
using MvvmGo.Models;
using System;
using System.Collections.Generic;

namespace MvvmGo.Validations
{
    public class ValidationsBuilder
    {
        public List<IValidationPropertyChanged> ViewModels { get; set; } = new List<IValidationPropertyChanged>();
        public List<Type> ModelTypes { get; set; } = new List<Type>();
        public List<string> Properties { get; set; } = new List<string>();
        public List<ValidationAttribute> Validations { get; set; } = new List<ValidationAttribute>();
    }
}
