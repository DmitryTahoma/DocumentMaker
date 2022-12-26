﻿using System;
using System.Globalization;
using System.Windows.Controls;

namespace Mvvm.ValidationRules
{
	public class DateTimeLessThenValidationRule : ValidationRule
	{
		public ContentProperties.String DateTimeString { get; set; } = null;

		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			if (DateTimeString == null)
				return new ValidationResult(false, "Не заданий " + nameof(DateTimeLessThenValidationRule) + "." + nameof(DateTimeString));

			return !DateTime.TryParse((value ?? "").ToString(), out DateTime dateTime)
				? new ValidationResult(false, "Формат повинен бути дд.мм.гггг.")
				: !DateTime.TryParse(DateTimeString.Value, out DateTime otherDateTime)
				? ValidationResult.ValidResult
				: dateTime >= otherDateTime
				? new ValidationResult(false, "Повинно бути менше " + otherDateTime.ToString("dd.MM.yyyy"))
				: ValidationResult.ValidResult;
		}
	}
}
