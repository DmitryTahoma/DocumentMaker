using System.Globalization;
using System.Windows.Controls;

namespace Mvvm.ValidationRules
{
	public class NotEmptyValidationRule : ValidationRule
	{
		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			return string.IsNullOrWhiteSpace((value ?? "").ToString())
				? new ValidationResult(false, "Поле має бути заповнене.")
				: ValidationResult.ValidResult;
		}
	}
}
