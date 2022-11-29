using Dml.Converters;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace Mvvm.ValidationRules
{
	public class FloatPositiveValidationRule : ValidationRule
	{
		Regex regexSpace = new Regex(@"\s", RegexOptions.Compiled);
		StringConverter stringConverter = new StringConverter();

		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			string str = (value ?? "").ToString();
			return str.Length > 0 && !regexSpace.IsMatch(str.Trim()) && stringConverter.TryConvertToFloat(str, out float t)
				? (t > 0 ? ValidationResult.ValidResult : new ValidationResult(false, "Число повинно бути більше 0."))
				: new ValidationResult(false, "Повинно бути дійсним числом.");
		}
	}
}
