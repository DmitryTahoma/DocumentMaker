using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dml.Controls
{
	/// <summary>
	/// Interaction logic for InputWithText.xaml
	/// </summary>
	public partial class InputWithText : UserControl
	{
		public static readonly DependencyProperty InputTextProperty;
		public static readonly DependencyProperty InputBackgroundProperty;
		public static readonly DependencyProperty InputReadOnlyProperty;

		static InputWithText()
		{
			InputTextProperty = DependencyProperty.Register("InputText", typeof(string), typeof(InputWithText));
			InputBackgroundProperty = DependencyProperty.Register("InputBackground", typeof(Brush), typeof(InputWithText));
			InputReadOnlyProperty = DependencyProperty.Register("InputReadOnly", typeof(bool), typeof(InputWithText));
		}

		public InputWithText()
		{
			InitializeComponent();
			DataContext = this;
		}

		public string TextInfo { get; set; }

		public string InputText
		{
			get => (string)GetValue(InputTextProperty);
			set => SetValue(InputTextProperty, value);
		}

		public Brush InputBackground
		{
			get => (Brush)GetValue(InputBackgroundProperty);
			set => SetValue(InputBackgroundProperty, value);
		}

		public bool InputReadOnly
		{
			get => (bool)GetValue(InputReadOnlyProperty);
			set => SetValue(InputReadOnlyProperty, value);
		}
	}
}
