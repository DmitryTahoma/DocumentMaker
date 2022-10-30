using Mvvm.ValidationRules.ContentProperties;
using System.Globalization;
using System.Windows.Controls;

namespace Mvvm.ValidationRules
{
	public class LessThanValidationRule : ValidationRule
	{
		public String String { get; set; } = null;

		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			if (String == null)
				return new ValidationResult(false, "Не заданий " + nameof(LessThanValidationRule) + "." + nameof(String));

			return !double.TryParse((value ?? "").ToString(), out double curNum)
				? new ValidationResult(false, "Повинно бути дійсним числом.")
				: !double.TryParse(String.Value, out double otherNum)
				? ValidationResult.ValidResult
				: curNum >= otherNum
				? new ValidationResult(false, "Повинно бути менше " + otherNum.ToString())
				: ValidationResult.ValidResult;
		}
	}
}
