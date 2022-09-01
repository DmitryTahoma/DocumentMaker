using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dml.Controller.Validation
{
	public class InputingValidator
	{
		private readonly Regex ufloatRegex;

		public InputingValidator()
		{
			ufloatRegex = new Regex(@"^\d*\.?\d*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		}

		public void UIntInputing_PreviewTextInput(object s, TextCompositionEventArgs e)
		{
			e.Handled = !uint.TryParse(e.Text, out uint _);
		}

		public void UFloatInputing_PreviewTextInput(object s, TextCompositionEventArgs e)
		{
			e.Handled = true;

			if (s is TextBox textBox)
			{
				string text = (textBox.Text + e.Text).Replace(',', '.');
				e.Handled = !ufloatRegex.IsMatch(text);
			}
		}

		public void BlockingCommand(object s, ExecutedRoutedEventArgs e)
		{
			e.Handled = true;
		}
	}
}
