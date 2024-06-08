using System.Globalization;
using System.Windows.Controls;

namespace TileEditor;

class IntValidator(int minValue, int maxValue, string errorMessage) : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (int.TryParse(value as string, out var parsed) && parsed >= minValue && parsed <= maxValue)
        {
            return ValidationResult.ValidResult;
        }

        return new ValidationResult(false, errorMessage);
    }
}
