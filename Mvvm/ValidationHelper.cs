using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Mvvm
{
	public static class ValidationHelper
	{
		public static bool IsValid(DependencyObject obj)
		{
			return !Validation.GetHasError(obj)
				&& LogicalTreeHelper.GetChildren(obj)
				   .OfType<DependencyObject>()
				   .All(IsValid);
		}
	}
}
