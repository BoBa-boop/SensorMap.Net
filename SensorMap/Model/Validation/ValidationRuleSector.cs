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
    class ValidationRuleSector:ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if(value == null) return new ValidationResult(false, "Отсутствует значение");
            var sector = (value as BindingGroup).Items[0] as Sector;
            if (sector != null)
            {
                if (string.IsNullOrWhiteSpace(sector!.Name))
                {
                    return new ValidationResult(false, "Поле 'Название' не может быть пустым!");
                }
                else sector.IsModified = true;
            }

            return ValidationResult.ValidResult;
        }
    }
}
        
