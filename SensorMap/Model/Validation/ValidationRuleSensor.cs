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
            var sensor = (value as BindingGroup).Items[0] as Sensor;
            if (sensor == null) return ValidationResult.ValidResult;
            if (ValidationHelper.IsEmptyName(sensor.Name))
            {
                return new ValidationResult(false, "Поле 'Название' не может быть пустым!");
            }
            if (ValidationHelper.ExceedsMaxLength(sensor.Name, 250))
            {
                return new ValidationResult(false, "Название не может быть длиннее 250 символов");
            }
            if (ValidationHelper.ContainsControlChars(sensor.Name))
            {
                return new ValidationResult(false, "Название не должно содержать служебные символы");
            }
            if (sensor.SensorType == null)
            {
                return new ValidationResult(false, "Тип обязателен для выбора");
            }
            sensor.IsModified = true;

            return ValidationResult.ValidResult;
        }
    }
}
