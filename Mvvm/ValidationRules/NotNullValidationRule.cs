using System.Globalization;
using System.Windows.Controls;

namespace Mvvm.ValidationRules
{
	public class NotNullValidationRule : ValidationRule
	{
		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			return value == null
				? new ValidationResult(false, "Значення не може бути невизначеним.")
				: ValidationResult.ValidResult;
		}
	}
}
