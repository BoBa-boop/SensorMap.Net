
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace SensorMap.Model.Validation
{
    class ValidationRuleMechanism : ValidationRule
    {
        //public int MyProperty { get; set; }
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null) return new ValidationResult(false, "Отсутствует значение");
            var mech = (value as BindingGroup).Items[0] as Mechanism;
            if (mech != null)
            {
                if (string.IsNullOrWhiteSpace(mech!.Name))
                {
                    return new ValidationResult(false, "Поле 'Название' не может быть пустым!");
                }
                else mech.IsModified = true;

                object sector = mech!.Sector;
                if (sector == null)
                {
                    return new ValidationResult(false, "Необходимо закрепить механизацию за участком!");
                }
                else mech.IsModified = true;
            }

            return ValidationResult.ValidResult;
        }
    }
}
