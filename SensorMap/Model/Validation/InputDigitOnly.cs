using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SensorMap.Model.Validation
{
    public class InputDigitOnly : ValidationRule
    {
        public bool IsIncludeSpecSymbols { get; set; }
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            string inputString = value as string ?? "";
            if (!IsIncludeSpecSymbols)
            {
                foreach (char ch in inputString)
                {
                    if (!char.IsDigit(ch))
                    {
                        return new ValidationResult(false, "Разрешены только цифры.");
                    }
                }
            }
            else
            {
                foreach (char ch in inputString)
                {
                    if (!Regex.IsMatch(inputString, @"^[0-9]+\.[0-9]+$"))
                    {
                        return new ValidationResult(false, "Ожидается ввод следующего формата '123.4'");
                    }
                }
            }

                return new ValidationResult(true, null);
        }
    }
}
