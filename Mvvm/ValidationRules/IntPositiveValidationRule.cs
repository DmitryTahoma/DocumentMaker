using System.Globalization;
using System.Windows.Controls;

namespace Mvvm.ValidationRules
{
	public class IntPositiveValidationRule : ValidationRule
	{
		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			return int.TryParse((value ?? "").ToString(), out int t)
				? (t > 0 ? ValidationResult.ValidResult : new ValidationResult(false, "Число повинно бути більше 0."))
				: new ValidationResult(false, "Повинно бути цілим числом.");
		}
	}
}
