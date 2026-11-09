using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace SensorMap.Model.Validation
{
    public class InputDigitOnly : ValidationRule
    {
        public bool IsIncludeSpecSymbols { get; set; }
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        { 
            string inputString = value as string ?? "";
            if (inputString.Length == 0) return new ValidationResult(false, "Строка не может быть пустой!");
            char separator = cultureInfo.NumberFormat.NumberDecimalSeparator[0];
            var pattern = $@"^\d+|{separator}\d+$";
            if (!IsIncludeSpecSymbols)
            {
                foreach (char ch in inputString)
                {
                    if (!char.IsDigit(ch))
                        return new ValidationResult(false, "Разрешены только цифры.");
                }
            }
            else
            {
                if (!Regex.IsMatch(inputString, pattern))
                {
                    return new ValidationResult(false, "Введите число в формате x.x");
                }
                if (!decimal.TryParse(inputString, NumberStyles.Number, cultureInfo, out _))
                    return new ValidationResult(false, "Неккоректное число");
            }
            return ValidationResult.ValidResult;
        }
    }
}
