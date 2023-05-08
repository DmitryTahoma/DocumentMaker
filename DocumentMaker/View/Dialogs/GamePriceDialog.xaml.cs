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
		public static readonly DependencyProperty GamePriceTextProperty;
		public static readonly DependencyProperty GamePriceAllTextProperty;

		public Dictionary<string, int> Games = new Dictionary<string, int>();
		static GamePriceDialog()
		{
			GamePriceTextProperty = DependencyProperty.Register("GamePriceText", typeof(string), typeof(GamePriceDialog));
			GamePriceAllTextProperty = DependencyProperty.Register("GamePriceAllText", typeof(string), typeof(GamePriceDialog));
		}
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

			GamePriceAllText = priceGame.ToString();
			GamePriceAllInput.Text = GamePriceAllText;
			GamePriceText = "0";

			GamesComboBox.SelectedIndex = 0;
		}

		public string GamePriceText
		{
			get => (string)GetValue(GamePriceTextProperty);
			set => SetValue(GamePriceTextProperty, value);
		}

		public string GamePriceAllText
		{
			get => (string)GetValue(GamePriceAllTextProperty);
			set => SetValue(GamePriceAllTextProperty, value);
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
					GamePriceText = Games[GamesComboBox.SelectedItem.ToString()].ToString();
				else
					GamePriceText = "0";
			}
			else
				GamePriceText = "0";

			GamePriceInput.Text = GamePriceText;
		}
	}
}
