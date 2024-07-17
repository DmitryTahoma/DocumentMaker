using MaterialDesignThemes.Wpf;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DocumentMakerModelLibrary;
using DocumentMakerModelLibrary.Algorithm;
using DocumentMakerModelLibrary.Controls;
using Dml.Model.Back;

namespace DocumentMaker.View.Dialogs
{
	/// <summary>
	/// Логика взаимодействия для GamePriceDialog.xaml
	/// </summary>
	public partial class GamePriceDialog : UserControl
	{
		public Dictionary<string, int> Games = new Dictionary<string, int>();

		public GamePriceDialog(IEnumerable<FullBackDataModel> _models, IList<GameObject> gameObjects)
		{
			InitializeComponent();
			DataContext = this;
			CalculateGamePrice.GamePrice(ref Games, _models, gameObjects, Dml.Model.Template.WorkType.All);
			int priceGame = 0;
			foreach(KeyValuePair<string, int> game in Games)
			{
				GamesComboBox.Items.Add(game.Key);
				priceGame += game.Value;
			}

			GamePriceAllInput.Text = priceGame.ToString();
			GamePriceInput.Text = "0";

			GamesComboBox.SelectedIndex = 0;
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
			GamesComboBox.Focus();
		}

		private void UnfocusOnEnter(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				Keyboard.ClearFocus();
			}
		}

		private void ChangedGame(object sender, SelectionChangedEventArgs e)
		{
			if (GamesComboBox.SelectedItem != null)
			{
				if(Games.ContainsKey(GamesComboBox.SelectedItem.ToString()))
					GamePriceInput.Text = Games[GamesComboBox.SelectedItem.ToString()].ToString();
				else
					GamePriceInput.Text = "0";
			}
			else
				GamePriceInput.Text = "0";
		}
	}
}
