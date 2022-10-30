using System.Windows;
using System.Windows.Data;

namespace Mvvm.ValidationRules.ContentProperties
{
	public abstract class ContentPropertyBase<T> : DependencyObject
	{
		public T Value
		{
			get { return (T)GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(T), typeof(ContentPropertyBase<T>), new PropertyMetadata(null, OnValueChanged));

		private static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			BindingExpressionBase bindingExpressionBase = BindingOperations.GetBindingExpressionBase(obj, BindingToTriggerProperty);
			bindingExpressionBase?.UpdateSource();
		}

		public object BindingToTrigger
		{
			get { return GetValue(BindingToTriggerProperty); }
			set { SetValue(BindingToTriggerProperty, value); }
		}
		public static readonly DependencyProperty BindingToTriggerProperty = DependencyProperty.Register(nameof(BindingToTrigger), typeof(object), typeof(ContentPropertyBase<T>), new FrameworkPropertyMetadata(default(object), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
	}
}
