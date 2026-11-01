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
            if (((BindingGroup)value).Items.Count == 0)
                return ValidationResult.ValidResult;
            var device = ((BindingGroup)value).Items[0] as Device;
            if (device!= null)
            {
                if (string.IsNullOrWhiteSpace(device!.Name))
                {
                    return new ValidationResult(false, "Поле 'Название' не может быть пустым!");
                }

                if (device.DeviceType == null)
                {
                    return new ValidationResult(false, "Тип обязателен для выбора");
                }
            }

            return ValidationResult.ValidResult;
        }
    }
}
