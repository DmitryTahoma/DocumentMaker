using System.Windows;
using System.Windows.Controls;

namespace ActGenerator.View.Dialogs
{
	/// <summary>
	/// Interaction logic for GeneratingProgressDialog.xaml
	/// </summary>
	public partial class GeneratingProgressDialog : UserControl
	{
		public GeneratingProgressDialog()
		{
			InitializeComponent();
		}

		#region Properties

		public string LabelText
		{
			get { return (string)GetValue(LabelTextProperty); }
			set { SetValue(LabelTextProperty, value); }
		}
		public static readonly DependencyProperty LabelTextProperty = DependencyProperty.Register(nameof(LabelText), typeof(string), typeof(GeneratingProgressDialog), new PropertyMetadata(string.Empty));

		public double ProgressValue
		{
			get { return (double)GetValue(ProgressValueProperty); }
			set { SetValue(ProgressValueProperty, value); }
		}
		public static readonly DependencyProperty ProgressValueProperty = DependencyProperty.Register(nameof(ProgressValue), typeof(double), typeof(GeneratingProgressDialog));

		public double ProgressMaximum
		{
			get { return (double)GetValue(ProgressMaximumProperty); }
			set { SetValue(ProgressMaximumProperty, value); }
		}
		public static readonly DependencyProperty ProgressMaximumProperty = DependencyProperty.Register(nameof(ProgressMaximum), typeof(double), typeof(GeneratingProgressDialog));

		#endregion
	}
}
