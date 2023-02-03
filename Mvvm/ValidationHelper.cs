using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Mvvm
{
	public static class ValidationHelper
	{
		public static bool IsValid(DependencyObject obj, bool updateSource = false)
		{
			if (updateSource) UpdateSource(obj);

			return !Validation.GetHasError(obj)
				&& LogicalTreeHelper.GetChildren(obj)
				   .OfType<DependencyObject>()
				   .All((x) => IsValid(x, updateSource));
		}

		public static DependencyObject GetFirstInvalid(DependencyObject obj, bool updateSource = false)
		{
			if (updateSource) UpdateSource(obj);

			if (Validation.GetHasError(obj))
			{
				return obj;
			}
			else
			{
				foreach(DependencyObject innerObj in LogicalTreeHelper.GetChildren(obj).OfType<DependencyObject>())
				{
					DependencyObject firstInvalid = GetFirstInvalid(innerObj, updateSource);
					if (firstInvalid != null)
					{
						return firstInvalid;
					}
				}
			}
			return null;
		}

		private static void UpdateSource(DependencyObject obj)
		{
			if(obj is TextBox textBox)
			{
				textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
			}
			else if(obj is Selector selector)
			{
				selector.GetBindingExpression(Selector.SelectedItemProperty)?.UpdateSource();
				selector.GetBindingExpression(ItemsControl.ItemsSourceProperty)?.UpdateSource();
			}
			else if(obj is DatePicker datePicker)
			{
				datePicker.GetBindingExpression(DatePicker.TextProperty)?.UpdateSource();
			}
		}
	}
}
