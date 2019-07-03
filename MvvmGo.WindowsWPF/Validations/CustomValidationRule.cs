using MvvmGo.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MvvmGo.Validations
{
    public class CustomValidationRule : ValidationRule
    {
        ValidationMessageInfo ValidationMessageInfo { get; set; }
        public CustomValidationRule(ValidationMessageInfo validationMessageInfo)
        {
            ValidationMessageInfo = validationMessageInfo;
        }
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return new ValidationResult(false, ValidationMessageInfo.Message);
        }
    }
}
