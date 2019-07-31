using MvvmGo.Attributes;
using MvvmGo.Models;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public static ValidationsBuilder AddInstance<T>(this ValidationsBuilder builder, params T[] instances)
        {
            builder.ModelInstances.AddRange(instances.Cast<object>());
            return builder;
        }
        public static PropertyValidationsBuilder AddProperties(this ValidationsBuilder builder, params string[] propertyNames)
        {
            List<PropertyValidation> result = new List<PropertyValidation>();
            foreach (var item in propertyNames)
            {
                var property = builder.Properties.FirstOrDefault(x => x.Name == item);
                if (property == null)
                {
                    property = new PropertyValidation() { Name = item };
                    builder.Properties.Add(property);
                    result.Add(property);
                }
            }

            return new PropertyValidationsBuilder() { ValidationsBuilder = builder, PropertyValidation = result };
        }

        public static ValidationsBuilder AddValidations(this PropertyValidationsBuilder propertyBuilder, params ValidationAttribute[] validationAttributes)
        {
            foreach (var item in validationAttributes)
            {
                foreach (var property in propertyBuilder.PropertyValidation)
                {
                    property.Validations.Add(item);
                }
            }

            return propertyBuilder.ValidationsBuilder;
        }

        //public static ValidationsBuilder ClearValidationAndProperties(this ValidationsBuilder builder)
        //{
        //    //builder.Validations.Clear();
        //    builder.Properties.Clear();
        //    return builder;
        //}

        public static ValidationsBuilder Build(this ValidationsBuilder builder)//, bool clearValidationsAndProperties = false
        {
            foreach (var viewmodel in builder.ViewModels)
            {
                foreach (var modelType in builder.ModelTypes)
                {
                    foreach (var property in builder.Properties)
                    {
                        AddPropertyValidation(viewmodel, modelType, property, builder);
                    }
                }
                foreach (var modelInstance in builder.ModelInstances)
                {
                    foreach (var property in builder.Properties)
                    {
                        AddPropertyValidation(viewmodel, modelInstance, property, builder);
                    }
                }
            }
            //if (clearValidationsAndProperties)
            //    return builder.ClearValidationAndProperties();
            return builder;
        }

        public static Dictionary<IValidationPropertyChanged, Dictionary<object, Dictionary<string, ValidationsBuilder>>> PropertyValidations = new Dictionary<IValidationPropertyChanged, Dictionary<object, Dictionary<string, ValidationsBuilder>>>();
        internal static void AddPropertyValidation(IValidationPropertyChanged viewModel, object model, PropertyValidation property, ValidationsBuilder validationsBuilder)
        {
            if (PropertyValidations.ContainsKey(viewModel))
            {
                if (PropertyValidations.TryGetValue(viewModel, out Dictionary<object, Dictionary<string, ValidationsBuilder>> dictionary))
                {
                    if (dictionary.ContainsKey(model))
                    {
                        dictionary[model][property.Name] = validationsBuilder;
                    }
                    else
                    {
                        dictionary[model] = new Dictionary<string, ValidationsBuilder>() { { property.Name, validationsBuilder } };
                    }
                }
                else
                {
                    PropertyValidations.Add(viewModel, new Dictionary<object, Dictionary<string, ValidationsBuilder>>() { { model, new Dictionary<string, ValidationsBuilder>() { { property.Name, validationsBuilder } } } });
                }
            }
            else
            {
                PropertyValidations.Add(viewModel, new Dictionary<object, Dictionary<string, ValidationsBuilder>>() { { model, new Dictionary<string, ValidationsBuilder>() { { property.Name, validationsBuilder } } } });
            }
        }
    }
}
