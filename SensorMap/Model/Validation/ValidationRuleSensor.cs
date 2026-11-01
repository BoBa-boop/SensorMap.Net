using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace SensorMap.Model.Validation
{
    public class ValidationRuleSensor : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null) return new ValidationResult(false,"Отсутствует значение");
            if (((BindingGroup)value).Items.Count == 0)
                return ValidationResult.ValidResult;
            if (((BindingGroup)value).Items[0] is Sensor sensor)
            {
                if (string.IsNullOrWhiteSpace(sensor!.Name))
                {
                    return new ValidationResult(false, "Поле 'Название' не может быть пустым!");
                }
                if (sensor.SensorType == null)
                {
                    return new ValidationResult(false, "Тип обязателен для выбора");
                }
            }

            return ValidationResult.ValidResult;
        }
    }
}
