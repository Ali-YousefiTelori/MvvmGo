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
            foreach (var item in modelTypes)
            {
                if (!builder.ModelTypes.Contains(item))
                    builder.ModelTypes.AddRange(modelTypes);
            }
            return builder;
        }
        public static ValidationsBuilder AddInstances<T>(this ValidationsBuilder builder, params T[] instances)
        {
            foreach (var item in instances)
            {
                if (!builder.ModelInstances.Contains(item))
                    builder.ModelInstances.Add(item);
            }
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
                        AddPropertyValidation(viewmodel, builder);
                    }
                }
                foreach (var modelInstance in builder.ModelInstances)
                {
                    foreach (var property in builder.Properties)
                    {
                        AddPropertyValidation(viewmodel, builder);
                    }
                }
            }
            //if (clearValidationsAndProperties)
            //    return builder.ClearValidationAndProperties();
            return builder;
        }

        public static Dictionary<IValidationPropertyChanged, List<ValidationsBuilder>> PropertyValidations = new Dictionary<IValidationPropertyChanged, List<ValidationsBuilder>>();
        internal static void AddPropertyValidation(IValidationPropertyChanged viewModel, ValidationsBuilder validationsBuilder)
        {
            if (PropertyValidations.TryGetValue(viewModel, out List<ValidationsBuilder> items))
            {
                items.Add(validationsBuilder);
            }
            else
            {
                items = new List<ValidationsBuilder>() { validationsBuilder };
                PropertyValidations.Add(viewModel, items);
            }
        }
    }
}
