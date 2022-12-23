using System.Collections;
using System.Globalization;
using System.Windows.Controls;

namespace ActGenerator.ViewModel.ValidationRules
{
	public class NotEmptyIListValidationRule : ValidationRule
	{
		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			return value is IList list && list.Count > 0
				? ValidationResult.ValidResult
				: new ValidationResult(false, "Список не може бути пустим.");
		}
	}
}
