using System.Globalization;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Markup;
using Mvvm.ValidationRules.ContentProperties;

namespace Mvvm.ValidationRules
{
	[ContentProperty("ItemsSource")]
	public class SourceContainsValidationRule : ValidationRule
	{
		public ItemsSource ItemsSource { get; set; } = null;

		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			if (ItemsSource == null || ItemsSource.Value == null)
				return new ValidationResult(false, "Не заданий SourceContainsValidationRule.ItemsSource");

			return !ItemsSource.Value.OfType<object>().Contains(value)
				? new ValidationResult(false, "Значення не може бути невизначеним.")
				: ValidationResult.ValidResult;
		}
	}
}
