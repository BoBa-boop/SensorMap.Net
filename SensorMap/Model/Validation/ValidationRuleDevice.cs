using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace SensorMap.Model.Validation
{
    public class ValidationRuleDevice : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null) return new ValidationResult(false, "Отсутствует значение");
            var device = (value as BindingGroup).Items[0] as Device;
            if (device!= null)
            {
                if (string.IsNullOrWhiteSpace(device!.Name))
                {
                    return new ValidationResult(false, "Поле 'Название' не может быть пустым!");
                }

                object type = device!.DeviceType;
                if (type == null)
                {
                    return new ValidationResult(false, "Тип обязателен для выбора");
                }

                if (type.GetType().IsEnum)
                {
                    var defaultValue = Activator.CreateInstance(type.GetType());
                    if (type.Equals(defaultValue))
                    {
                        return new ValidationResult(false, "Выберите тип устройства");
                    }
                }
            }

            return ValidationResult.ValidResult;
        }
    }
}
