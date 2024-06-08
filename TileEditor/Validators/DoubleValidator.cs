using System.Globalization;
using System.Windows.Controls;

namespace TileEditor;

class DoubleValidator : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (!double.TryParse(value as string, out var _))
        {
            return new ValidationResult(false, "Enter a valid double");
        }

        return ValidationResult.ValidResult;            
    }
}
