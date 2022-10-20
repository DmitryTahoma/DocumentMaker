using System.Globalization;
using System.Windows.Controls;

namespace Mvvm.ValidationRules
{
	public class FloatPositiveValidationRule : ValidationRule
	{
		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			string str = (value ?? "").ToString();
			return str.Length > 0 && char.IsDigit(str[0]) && float.TryParse(str, out float t)
				? (t > 0 ? ValidationResult.ValidResult : new ValidationResult(false, "Число повинно бути більше 0."))
				: new ValidationResult(false, "Повинно бути дійсним числом.");
		}
	}
}
