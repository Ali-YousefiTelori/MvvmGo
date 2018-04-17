using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MvvmGo.Models
{
    public enum ValidationMessageType
    {
        None = 0,
        Error = 1,
        Warning = 2,
        Information = 3
    }

    public class ValidationMessageInfo
    {
        public ValidationMessageType Type { get; set; }
        public string Message { get; set; }
        public int Count { get; set; }

        public static implicit operator ValidationMessageInfo(string message)
        {
            return new ValidationMessageInfo() { Message = message, Type = ValidationMessageType.Error };
        }
    }
}
