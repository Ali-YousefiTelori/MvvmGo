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

        public static PropertyValidationsBuilder AddProperties(this ValidationsBuilder builder, params string[] propertyNames)
        {
            List<PropertyValidation> result = new List<PropertyValidation>();
            foreach (var item in propertyNames)
            {
                var property = builder.Properties.FirstOrDefault(x => x.Name== item);
                if (property == null)
                {
                    property = new PropertyValidation() { Name = item };
                    builder.Properties.Add(property);
                    result.Add(property);
                }
            }

            return new PropertyValidationsBuilder() { ValidationsBuilder = builder, PropertyValidation = result };
        }

        public static ValidationsBuilder AddValidations(this PropertyValidationsBuilder  propertyBuilder, params ValidationAttribute[] validationAttributes)
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
            }
            //if (clearValidationsAndProperties)
            //    return builder.ClearValidationAndProperties();
            return builder;
        }

        public static Dictionary<IValidationPropertyChanged, Dictionary<Type, Dictionary<string, ValidationsBuilder>>> PropertyValidations = new Dictionary<IValidationPropertyChanged, Dictionary<Type, Dictionary<string, ValidationsBuilder>>>();
        internal static void AddPropertyValidation(IValidationPropertyChanged viewModel, Type modelType, PropertyValidation property, ValidationsBuilder validationsBuilder)
        {
            if (PropertyValidations.ContainsKey(viewModel))
            {
                if (PropertyValidations.TryGetValue(viewModel, out Dictionary<Type, Dictionary<string,ValidationsBuilder>> dictionary))
                {
                    if (dictionary.ContainsKey(modelType))
                    {
                        dictionary[modelType][property.Name] = validationsBuilder;
                    }
                    else
                    {
                        dictionary[modelType] = new Dictionary<string, ValidationsBuilder>() { { property.Name, validationsBuilder } };
                    }
                }
                else
                {
                    PropertyValidations.Add(viewModel, new Dictionary<Type, Dictionary<string, ValidationsBuilder>>() { { modelType, new Dictionary<string, ValidationsBuilder>() { { property.Name, validationsBuilder } } } });
                }
            }
            else
            {
                PropertyValidations.Add(viewModel, new Dictionary<Type, Dictionary<string, ValidationsBuilder>>() { { modelType, new Dictionary<string, ValidationsBuilder>() { { property.Name, validationsBuilder } } } });
            }
        }
    }
}
