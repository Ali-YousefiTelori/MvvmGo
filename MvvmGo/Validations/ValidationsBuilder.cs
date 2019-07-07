﻿using MvvmGo.Attributes;
using MvvmGo.Models;
using System;
using System.Collections.Generic;

namespace MvvmGo.Validations
{
    public class PropertyValidationsBuilder
    {
        public ValidationsBuilder ValidationsBuilder { get; set; }
        public List<PropertyValidation> PropertyValidation { get; set; }
    }

    public class ValidationsBuilder
    {
        bool _HasError;
        public bool HasError
        {
            get
            {
                return _HasError;
            }

            set
            {
                _HasError = value;
                foreach (var item in ViewModels)
                {
                    item.OnPropertyChanged(nameof(item.HasError));
                    item.ValidateCanExecute();
                }
            }
        }
        public List<IValidationPropertyChanged> ViewModels { get; set; } = new List<IValidationPropertyChanged>();
        public List<Type> ModelTypes { get; set; } = new List<Type>();
        public List<PropertyValidation> Properties { get; set; } = new List<PropertyValidation>();
    }

    public class PropertyValidation
    {
        public string Name { get; set; }
        public List<ValidationAttribute> Validations { get; set; } = new List<ValidationAttribute>();
    }
}