using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Data;

namespace SensorMap.Model.Validation
{
    public class InputDigitOnly : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            // 1. Извлекаем фактическое значение, если пришёл BindingExpression
            object actualValue = value;
            if (value is BindingExpression binding)
            {
                var source = binding.ResolvedSource;
                var propName = binding.ResolvedSourcePropertyName;
                if (source != null && !string.IsNullOrEmpty(propName))
                {
                    var prop = source.GetType().GetProperty(propName);
                    if (prop != null)
                        actualValue = prop.GetValue(source, null);
                }
            }
            if(actualValue == null) return ValidationResult.ValidResult;
            
            string text = actualValue.ToString()?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(text)) return ValidationResult.ValidResult;
            // 3. Проверяем формат: целая часть — 1+ цифр, затем разделитель, затем дробная часть
            // Разделитель берём из текущей культуры ('.' или ',')
            char separator = cultureInfo.NumberFormat.NumberDecimalSeparator[0];

            // Регулярка: хотя бы одна цифра слева, разделитель, хотя бы одна цифра справа
            // ^\d+ — любое количество цифр в целой части
            // \d+$ — хотя бы одна цифра в дробной части
            var pattern = $@"^\d+\{separator}\d+$";

            if (!System.Text.RegularExpressions.Regex.IsMatch(text, pattern))
                return new ValidationResult(false, "Число должно быть в формате y.x (например, 12.5)");

            // 4. Дополнительно убеждаемся, что число парсится (на случай переполнения и т.п.)
            if (!decimal.TryParse(text, NumberStyles.Number, cultureInfo, out _))
                return new ValidationResult(false, "Некорректное число");

            return ValidationResult.ValidResult;
        }

    }
}
