using System.Windows;

namespace ActGenerator.ViewModel.Dialogs
{
	public class GenerationDialogViewModel : DependencyObject
	{
		#region Properties

		public string LabelText
		{
			get { return (string)GetValue(LabelTextProperty); }
			set { SetValue(LabelTextProperty, value); }
		}
		public static readonly DependencyProperty LabelTextProperty = DependencyProperty.Register(nameof(LabelText), typeof(string), typeof(GenerationDialogViewModel), new PropertyMetadata(string.Empty));

		public double ProgressValue
		{
			get { return (double)GetValue(ProgressValueProperty); }
			set { SetValue(ProgressValueProperty, value); }
		}
		public static readonly DependencyProperty ProgressValueProperty = DependencyProperty.Register(nameof(ProgressValue), typeof(double), typeof(GenerationDialogViewModel));

		public double ProgressMaximum
		{
			get { return (double)GetValue(ProgressMaximumProperty); }
			set { SetValue(ProgressMaximumProperty, value); }
		}
		public static readonly DependencyProperty ProgressMaximumProperty = DependencyProperty.Register(nameof(ProgressMaximum), typeof(double), typeof(GenerationDialogViewModel));

		#endregion
	}
}
