using System.Windows.Input;

namespace Dml.Controller.Validation
{
	public class InputingValidator
	{
		public void UIntInputing_PreviewTextInput(object s, TextCompositionEventArgs e)
		{
			e.Handled = !uint.TryParse(e.Text, out uint _);
		}

		public void BlockingCommand(object s, ExecutedRoutedEventArgs e)
		{
			e.Handled = true;
		}
	}
}
