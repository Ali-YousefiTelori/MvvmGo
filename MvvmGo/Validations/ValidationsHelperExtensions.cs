using MvvmGo.Attributes;
using MvvmGo.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MvvmGo.Validations
{
    public static class ValidationsHelperExtensions
    {
        public static ValidationsGenerator AddViewModels(this ValidationsGenerator builder, params IValidationPropertyChanged[] viewModels)
        {
            builder.ViewModels.AddRange(viewModels);
            return builder;
        }

        public static ValidationsGenerator AddTypes(this ValidationsGenerator builder, params Type[] modelTypes)
        {
            foreach (var item in modelTypes)
            {
                if (!builder.ModelTypes.TryGetValue(item, out List<PropertyValidation> validations))
                    builder.ModelTypes.Add(item, new List<PropertyValidation>());
            }
            return builder;
        }
        public static ValidationsGenerator AddInstances<T>(this ValidationsGenerator builder, params T[] instances)
        {
            foreach (var item in instances)
            {
                if (item == null)
                    throw new Exception("Your Instance is null, you cannot add validation for a null instance");
                if (!builder.ModelInstances.TryGetValue(item, out List<PropertyValidation> validations))
                    builder.ModelInstances.Add(item, new List<PropertyValidation>());
            }
            return builder;
        }

        public static PropertyValidationsGenerator AddProperties(this ValidationsGenerator builder, params string[] propertyNames)
        {
            List<PropertyValidation> result = new List<PropertyValidation>();
            foreach (var instance in builder.ModelInstances)
            {
                foreach (var item in propertyNames)
                {
                    var property = instance.Value.FirstOrDefault(x => x.Name == item);
                    if (property == null)
                    {
                        property = new PropertyValidation() { Name = item };
                        instance.Value.Add(property);
                    }
                    result.Add(property);
                }
            }

            foreach (var type in builder.ModelTypes)
            {
                foreach (var item in propertyNames)
                {
                    var property = type.Value.FirstOrDefault(x => x.Name == item);
                    if (property == null)
                    {
                        property = new PropertyValidation() { Name = item };
                        type.Value.Add(property);
                    }
                    result.Add(property);
                }
            }

            return new PropertyValidationsGenerator() { ValidationsBuilder = builder, PropertyValidation = result };
        }

        public static ValidationsGenerator AddValidations(this PropertyValidationsGenerator propertyBuilder, params ValidationAttribute[] validationAttributes)
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

        public static ValidationsBuilder ClearValidations(this ValidationsBuilder builder)
        {
            foreach (var item in builder.ModelInstances)
            {
                item.Value.Clear();
            }
            foreach (var item in builder.ModelTypes)
            {
                item.Value.Clear();
            }
            return builder;
        }

        public static ValidationsGenerator Build(this ValidationsGenerator generator, ValidationsBuilder builder)//, bool clearValidationsAndProperties = false
        {
            foreach (var item in generator.ModelInstances)
            {
                if (!builder.ModelInstances.ContainsKey(item.Key))
                    builder.ModelInstances.Add(item.Key, item.Value);
            }
            foreach (var item in generator.ViewModels)
            {
                if (!builder.ViewModels.Contains(item))
                    builder.ViewModels.Add(item);
            }
            foreach (var item in generator.ModelTypes)
            {
                if (!builder.ModelTypes.ContainsKey(item.Key))
                    builder.ModelTypes.Add(item.Key, item.Value);
            }
            builder.RealTimeCheck = generator.RealTimeCheck;
            foreach (var viewmodel in generator.ViewModels)
            {
                AddPropertyValidation(viewmodel, builder);
            }

            generator.ModelInstances.Clear();
            generator.ModelTypes.Clear();
            return generator;
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
