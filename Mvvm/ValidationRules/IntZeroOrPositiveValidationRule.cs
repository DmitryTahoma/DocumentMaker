using System.Globalization;
using System.Windows.Controls;

namespace Mvvm.ValidationRules
{
	public class IntZeroOrPositiveValidationRule : ValidationRule
	{
		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			return int.TryParse((value ?? "").ToString(), out int t)
				? (t >= 0 ? ValidationResult.ValidResult : new ValidationResult(false, "Число повинно бути більше або дорівнювати 0."))
				: new ValidationResult(false, "Повинно бути числом.");
		}
	}
}
