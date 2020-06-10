using MvvmGo.Attributes;
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
        public static Action<ValidationsBuilder> Changed { get; set; }

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
        public Dictionary<Type, List<PropertyValidation>> ModelTypes { get; set; } = new Dictionary<Type, List<PropertyValidation>>();
        public Dictionary<object, List<PropertyValidation>> ModelInstances { get; set; } = new Dictionary<object, List<PropertyValidation>>();
        public bool RealTimeCheck { get; set; } = true;
        
        public bool LockHasError { get; set; }
        public bool LockHasErrorWasTrue { get; set; }
        public void Validate()
        {
            for (int i = 0; i < 2; i++)
            {
                _HasError = false;
                LockHasErrorWasTrue = false;
                LockHasError = true;
                Changed?.Invoke(this);
                LockHasError = false;
            }
        }
    }
    public class PropertyValidationsGenerator
    {
        public ValidationsGenerator ValidationsBuilder { get; set; }
        public List<PropertyValidation> PropertyValidation { get; set; }
    }

    public class ValidationsGenerator
    {
        public List<IValidationPropertyChanged> ViewModels { get; set; } = new List<IValidationPropertyChanged>();
        public Dictionary<Type, List<PropertyValidation>> ModelTypes { get; set; } = new Dictionary<Type, List<PropertyValidation>>();
        public Dictionary<object, List<PropertyValidation>> ModelInstances { get; set; } = new Dictionary<object, List<PropertyValidation>>();
        public bool RealTimeCheck { get; set; } = true;

        //public static implicit operator ValidationsBuilder(ValidationsGenerator validationsGenerator)
        //{
        //    return new ValidationsBuilder()
        //    {
        //        ViewModels = validationsGenerator.ViewModels,
        //        ModelInstances = validationsGenerator.ModelInstances,
        //        ModelTypes = validationsGenerator.ModelTypes,
        //        RealTimeCheck = validationsGenerator.RealTimeCheck
        //    };
        //}

        //public static implicit operator ValidationsGenerator(ValidationsBuilder validationsBuilder)
        //{
        //    var result = new ValidationsBuilder();
        //    foreach (var item in validationsBuilder.ModelInstances)
        //    {
        //        result.ModelInstances.Add(item.Key, item.Value);
        //    }
        //    foreach (var item in validationsBuilder.ViewModels)
        //    {
        //        result.ViewModels.Add(item);
        //    }
        //    foreach (var item in validationsBuilder.ModelTypes)
        //    {
        //        result.ModelTypes.Add(item.Key, item.Value);
        //    }
        //    result.RealTimeCheck = validationsBuilder.RealTimeCheck;
        //    return result;
        //}
    }

    public class PropertyValidation
    {
        public string Name { get; set; }
        public List<ValidationAttribute> Validations { get; set; } = new List<ValidationAttribute>();
    }
}
