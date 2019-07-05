using MvvmGo.Attributes;
using MvvmGo.Models;
using System;
using System.Collections.Generic;

namespace MvvmGo.Validations
{
    public static class ValidationsHelperExtensions
    {
        public static ValidationsBuilder AddViewModels(this ValidationsBuilder builder, params IValidationPropertyChanged[] viewModels)
        {
            builder.ViewModels.AddRange(viewModels);
            return builder;
        }

        public static ValidationsBuilder AddTypes(this ValidationsBuilder builder, params Type[] modelTypes)
        {
            builder.ModelTypes.AddRange(modelTypes);
            return builder;
        }

        public static ValidationsBuilder AddProperties(this ValidationsBuilder builder, params string[] propertyNames)
        {
            builder.Properties.AddRange(propertyNames);
            return builder;
        }

        public static ValidationsBuilder AddValidations(this ValidationsBuilder builder, params ValidationAttribute[] validationAttributes)
        {
            builder.Validations.AddRange(validationAttributes);
            return builder;
        }

        public static ValidationsBuilder ClearValidationAndProperties(this ValidationsBuilder builder)
        {
            builder.Validations.Clear();
            builder.Properties.Clear();
            return builder;
        }

        public static ValidationsBuilder Build(this ValidationsBuilder builder, bool clearValidationsAndProperties = false)
        {
            foreach (var viewmodel in builder.ViewModels)
            {
                foreach (var modelType in builder.ModelTypes)
                {
                    foreach (var property in builder.Properties)
                    {
                        AddPropertyValidation(viewmodel, modelType, property, builder.Validations.ToArray());
                    }
                }
            }
            if (clearValidationsAndProperties)
                return builder.ClearValidationAndProperties();
            return builder;
        }

        public static Dictionary<IValidationPropertyChanged, Dictionary<Type, Dictionary<string, List<ValidationAttribute>>>> PropertyValidations = new Dictionary<IValidationPropertyChanged, Dictionary<Type, Dictionary<string, List<ValidationAttribute>>>>();
        internal static void AddPropertyValidation(IValidationPropertyChanged viewModel, Type modelType, string propertyName, params ValidationAttribute[] validationAttributes)
        {
            if (PropertyValidations.ContainsKey(viewModel))
            {
                if (PropertyValidations.TryGetValue(viewModel, out Dictionary<Type, Dictionary<string, List<ValidationAttribute>>> dictionary))
                {
                    if (dictionary.ContainsKey(modelType))
                    {
                        if (!dictionary[modelType].ContainsKey(propertyName))
                            dictionary[modelType][propertyName] = new List<ValidationAttribute>(validationAttributes);
                        else
                            dictionary[modelType][propertyName].AddRange(validationAttributes);
                    }
                    else
                    {
                        dictionary[modelType] = new Dictionary<string, List<ValidationAttribute>>() { { propertyName, new List<ValidationAttribute>(validationAttributes) } };
                    }
                }
                else
                {
                    PropertyValidations.Add(viewModel, new Dictionary<Type, Dictionary<string, List<ValidationAttribute>>>() { { modelType, new Dictionary<string, List<ValidationAttribute>>() { { propertyName, new List<ValidationAttribute>(validationAttributes) } } } });
                }
            }
            else
            {
                PropertyValidations.Add(viewModel, new Dictionary<Type, Dictionary<string, List<ValidationAttribute>>>() { { modelType, new Dictionary<string, List<ValidationAttribute>>() { { propertyName, new List<ValidationAttribute>(validationAttributes) } } } });
            }
        }
    }
}
