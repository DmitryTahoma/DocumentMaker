using Dml.Model.Back;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DocumentMaker.View.Dialogs
{
	/// <summary>
	/// Interaction logic for ChangeGamesNamesDialog.xaml
	/// </summary>
	public partial class ChangeGamesNamesDialog : UserControl
	{
		public ChangeGamesNamesDialog()
		{
			InitializeComponent();
			DataContext = this;
		}

		public static readonly DependencyProperty GameNameListProperty = DependencyProperty.Register(nameof(GameNameList), typeof(List<GameObject>), typeof(ChangeGamesNamesDialog));
		public List<GameObject> GameNameList
		{
			get { return (List<GameObject>)GetValue(GameNameListProperty); }
			set { SetValue(GameNameListProperty, value); }
		}

		public static readonly DependencyProperty SelectedGameNameProperty = DependencyProperty.Register(nameof(SelectedGameName), typeof(string), typeof(ChangeGamesNamesDialog), new PropertyMetadata(string.Empty));
		public string SelectedGameName
		{
			get { return (string)GetValue(SelectedGameNameProperty); }
			set { SetValue(SelectedGameNameProperty, value); }
		}

		public static readonly DependencyProperty NewGameNameProperty = DependencyProperty.Register(nameof(NewGameName), typeof(string), typeof(ChangeGamesNamesDialog), new PropertyMetadata(string.Empty));
		public string NewGameName
		{
			get { return (string)GetValue(NewGameNameProperty); }
			set { SetValue(NewGameNameProperty, value); }
		}

		public bool IsChanging { get; private set; } = false;

		private void ChangeGameNameClick(object sender, RoutedEventArgs e)
		{
			IsChanging = true;
			DialogHost.CloseDialogCommand.Execute(null, null);
		}

		private void ControlKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
			{
				DialogHost.CloseDialogCommand.Execute(null, null);
			}
		}

		private void DialogLoaded(object sender, RoutedEventArgs e)
		{
			SourceGameNameComboBox.Focus();
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
