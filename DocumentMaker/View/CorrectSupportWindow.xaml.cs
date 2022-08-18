using Dml.Controller.Validation;
using System.Windows;
using System.Windows.Input;

namespace DocumentMaker.View
{
	/// <summary>
	/// Interaction logic for CorrectSupportWindow.xaml
	/// </summary>
	public partial class CorrectSupportWindow : Window
	{
		private readonly InputingValidator inputingValidator;

		public CorrectSupportWindow()
		{
			InitializeComponent();
			DataContext = this;

			inputingValidator = new InputingValidator();
			SumInput.CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, inputingValidator.BlockingCommand));
		}

		public bool IsCorrection { get; private set; }
		public string NumberText { get; set; }
		public bool TakeSumFromDevelopment { get; set; }

		private void WindowKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
			{
				Close();
			}
		}

		private void CorrectionClick(object sender, RoutedEventArgs e)
		{
			IsCorrection = true;
			Close();
		}

		private void UIntValidating(object sender, System.Windows.Input.TextCompositionEventArgs e)
		{
			inputingValidator.UIntInputing_PreviewTextInput(sender, e);
		}
	}
}
