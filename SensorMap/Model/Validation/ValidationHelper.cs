using System;
using System.Linq;

namespace SensorMap.Model.Validation
{
    internal static class ValidationHelper
    {
        public static bool IsEmptyName(string name) => string.IsNullOrWhiteSpace(name);

        public static bool ExceedsMaxLength(string name, int maxLength) =>
            !string.IsNullOrEmpty(name) && name.Length > maxLength;

        public static bool ContainsControlChars(string name) =>
            !string.IsNullOrEmpty(name) && name.Any(c => char.IsControl(c));
    }
}