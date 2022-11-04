using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Mvvm.Converters
{
	[ValueConversion(typeof(ViewModelState), typeof(Visibility))]
	public class ViewModelStateToVisibilityConverter : IValueConverter
	{
		public ViewModelState VisibleValue { get; set; } = ViewModelState.Loading;
		public bool IsInverted { get; set; } = false;
		public Visibility FalseValue { get; set; } = Visibility.Collapsed;

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is ViewModelState viewModelState)
			{
				return (!IsInverted && viewModelState == VisibleValue) || (IsInverted && viewModelState != VisibleValue)
					? Visibility.Visible
					: FalseValue;
			}

			throw new ArgumentException();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Binding.DoNothing;
		}
	}
}
