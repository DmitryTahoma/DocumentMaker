using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DocumentMaker.View.Dialogs
{
	/// <summary>
	/// Interaction logic for ChangeContractFilesDialog.xaml
	/// </summary>
	public partial class ChangeContractFilesDialog : UserControl
	{
		public ChangeContractFilesDialog(ComboBox ContractFiles)
		{
			InitializeComponent();
			DataContext = this;

			foreach(string name in ContractFiles.Items)
			{
				ContractFilesComboBox.Items.Add(name);
			}

			if (ContractFiles.SelectedItem != null)
				ContractFilesComboBox.SelectedItem = ContractFiles.SelectedItem;
			else
				ContractFilesComboBox.SelectedIndex = 0;
		}

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
			ContractFilesComboBox.Focus();
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
