
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace SensorMap.Model.Validation
{
    class ValidationRuleMechanism : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null) return new ValidationResult(false, "Отсутствует значение");
            var mech = (value as BindingGroup).Items[0] as Mechanism;
            if (mech == null) return ValidationResult.ValidResult;
            if (ValidationHelper.IsEmptyName(mech.Name))
            {
                return new ValidationResult(false, "Поле 'Название' не может быть пустым!");
            }
            if (ValidationHelper.ExceedsMaxLength(mech.Name, 250))
            {
                return new ValidationResult(false, "Название не может быть длиннее 250 символов");
            }
            if (ValidationHelper.ContainsControlChars(mech.Name))
            {
                return new ValidationResult(false, "Название не должно содержать служебные символы");
            }
            if (mech.Sector == null)
            {
                return new ValidationResult(false, "Необходимо закрепить механизацию за участком!");
            }
            mech.IsModified = true;

            return ValidationResult.ValidResult;
        }
    }
}
