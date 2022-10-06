using System.Collections;
using System.Windows;
using System.Windows.Data;

namespace Mvvm.ValidationRules.ContentProperties
{
	public class ItemsSource : DependencyObject
	{
		public IEnumerable Value
		{
			get { return (IEnumerable)GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(IEnumerable), typeof(ItemsSource), new PropertyMetadata(null, OnValueChanged));

		private static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ItemsSource itemsSource = (ItemsSource)obj;
			BindingExpressionBase bindingExpressionBase = BindingOperations.GetBindingExpressionBase(itemsSource, BindingToTriggerProperty);
			bindingExpressionBase?.UpdateSource();
		}

		public object BindingToTrigger
		{
			get { return GetValue(BindingToTriggerProperty); }
			set { SetValue(BindingToTriggerProperty, value); }
		}
		public static readonly DependencyProperty BindingToTriggerProperty = DependencyProperty.Register(nameof(BindingToTrigger), typeof(object), typeof(ItemsSource), new FrameworkPropertyMetadata(default(object), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
	}
}
