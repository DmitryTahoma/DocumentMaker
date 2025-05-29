using Dml.Model.Back;
using DocumentFormat.OpenXml.Wordprocessing;
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

		public static readonly DependencyProperty SelectedEpisodeProperty = DependencyProperty.Register(nameof(SelectedEpisode), typeof(string), typeof(ChangeGamesNamesDialog));
		public string SelectedEpisode
		{
			get { return (string)GetValue(SelectedEpisodeProperty); }
			set { SetValue(SelectedEpisodeProperty, value); }
		}

		public static readonly DependencyProperty NewEpisodeProperty = DependencyProperty.Register(nameof(NewEpisode), typeof(string), typeof(ChangeGamesNamesDialog));
		public string NewEpisode
		{
			get { return (string)GetValue(NewEpisodeProperty); }
			set { SetValue(NewEpisodeProperty, value); }
		}

		public static readonly DependencyProperty EpisodesListProperty = DependencyProperty.Register(nameof(EpisodesList), typeof(List<string>), typeof(ChangeGamesNamesDialog));
		public List<string> EpisodesList
		{
			get { return (List<string>)GetValue(EpisodesListProperty); }
			set { SetValue(EpisodesListProperty, value); }
		}

		public static readonly DependencyProperty NewEpisodesListProperty = DependencyProperty.Register(nameof(NewEpisodesList), typeof(List<string>), typeof(ChangeGamesNamesDialog));
		public List<string> NewEpisodesList
		{
			get { return (List<string>)GetValue(NewEpisodesListProperty); }
			set { SetValue(NewEpisodesListProperty, value); }
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

		private void SourceGameNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count <= 0)
				EpisodesList = null;
			else
				EpisodesList = GameNameList.FirstOrDefault(x => x.Name == e.AddedItems[0].ToString())?.Episodes.ToList();

			if (EpisodesList == null)
				SelectedEpisode = null;
		}

		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count <= 0)
				EpisodesList = null;
			else
				NewEpisodesList = GameNameList.FirstOrDefault(x => x.Name == e.AddedItems[0].ToString())?.Episodes.ToList();

			if (NewEpisodesList == null)
				NewEpisode = null;
		}
	}
}
