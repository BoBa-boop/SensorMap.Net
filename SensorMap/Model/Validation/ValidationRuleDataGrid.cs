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
    public class ValidationRuleDataGrid : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var sensor = (value as BindingGroup).Items[0] as Sensor;
            if (string.IsNullOrWhiteSpace(sensor.Name))
            {
                return new ValidationResult(false,"Поле 'Название' не может быть пустым!");
            }
            else sensor.IsModified = true;

            object type = sensor?.Type;
            if (type == null)
            {
                return new ValidationResult(false, "Тип обязателен для выбора");
            }
            else sensor.IsModified = true;

            if (type.GetType().IsEnum)
            {
                var defaultValue = Activator.CreateInstance(type.GetType());
                if (type.Equals(defaultValue))
                {
                    return new ValidationResult(false, "Выберите тип датчика");
                }
            }
            else sensor.IsModified = true;

            return ValidationResult.ValidResult;
        }
    }
}
