using System.Globalization;
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
            if (device == null) return ValidationResult.ValidResult;
            if (ValidationHelper.IsEmptyName(device.Name))
            {
                return new ValidationResult(false, "Поле 'Название' не может быть пустым!");
            }
            if (ValidationHelper.ExceedsMaxLength(device.Name, 250))
            {
                return new ValidationResult(false, "Название не может быть длиннее 250 символов");
            }
            if (ValidationHelper.ContainsControlChars(device.Name))
            {
                return new ValidationResult(false, "Название не должно содержать служебные символы");
            }
            if (device.DeviceType == null)
            {
                return new ValidationResult(false, "Тип обязателен для выбора");
            }
            device.IsModified = true;

            return ValidationResult.ValidResult;
        }
    }
}
