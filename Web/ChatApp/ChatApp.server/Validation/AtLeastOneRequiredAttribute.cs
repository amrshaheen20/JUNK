using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ChatApi.server.Validation
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class AtLeastOneRequiredAttribute : ValidationAttribute
    {
        private readonly string[] _propertyNames;

        public AtLeastOneRequiredAttribute(params string[] propertyNames)
        {
            _propertyNames = propertyNames;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult("Object cannot be null.");
            }

            Type type = value.GetType();
            bool isValid = _propertyNames.Any(propertyName =>
            {
                PropertyInfo? property = type.GetProperty(propertyName);
                if (property == null)
                    return false;

                object? propertyValue = property.GetValue(value);
                return propertyValue switch
                {
                    string str => !string.IsNullOrWhiteSpace(str),
                    ICollection<object> collection => collection.Count > 0,
                    _ => propertyValue != null
                };
            });

            return isValid ? ValidationResult.Success : new ValidationResult($"At least one of the following properties must be provided: {string.Join(", ", _propertyNames)}.", _propertyNames);
        }
    }

}
