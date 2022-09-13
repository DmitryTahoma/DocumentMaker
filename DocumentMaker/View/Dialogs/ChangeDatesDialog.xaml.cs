using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DocumentMaker.View.Dialogs
{
	/// <summary>
	/// Interaction logic for ChangeDatesDialog.xaml
	/// </summary>
	public partial class ChangeDatesDialog : UserControl
	{
		public ChangeDatesDialog()
		{
			InitializeComponent();
			DataContext = this;
		}

		public string TechnicalTaskDateText { get; set; }

		public string ActDateText { get; set; }

		public bool IsChanging { get; private set; }

		private void ControlKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
			{
				DialogHost.CloseDialogCommand.Execute(null, null);
			}
		}

		private void ChangeDatesClick(object sender, RoutedEventArgs e)
		{
			IsChanging = true;
			DialogHost.CloseDialogCommand.Execute(null, null);
		}

		private void DialogLoaded(object sender, RoutedEventArgs e)
		{
			TechnicalTaskDatePicker.Focus();
		}

		private void UnfocusOnEnter(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				Keyboard.ClearFocus();
			}
		}
	}
}
