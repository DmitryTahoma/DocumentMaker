using Dml.Controller.Validation;
using System.Windows;
using System.Windows.Input;

namespace DocumentMaker.View
{
	/// <summary>
	/// Interaction logic for CorrectDevelopmentWindow.xaml
	/// </summary>
	public partial class CorrectDevelopmentWindow : Window
	{
		private readonly InputingValidator inputingValidator;

		public CorrectDevelopmentWindow()
		{
			InitializeComponent();
			DataContext = this;

			inputingValidator = new InputingValidator();
			SumInput.CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, inputingValidator.BlockingCommand));
		}

		public bool IsCorrection { get; private set; }
		public string NumberText { get; set; }
		public bool TakeSumFromSupport { get; set; }

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
