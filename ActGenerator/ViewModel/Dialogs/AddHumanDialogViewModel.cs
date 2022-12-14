﻿using ActGenerator.Model.Dialogs;
using DocumentMakerModelLibrary.OfficeFiles.Human;
using Mvvm;
using Mvvm.Commands;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ActGenerator.ViewModel.Dialogs
{
	public class AddHumanDialogViewModel : DependencyObject
	{
		AddHumanDialogModel model = new AddHumanDialogModel();

		Grid humenGrid = null;

		public AddHumanDialogViewModel()
		{
			InitCommands();

			State = ViewModelState.Initialized;
		}

		#region Properties

		public ViewModelState State
		{
			get { return (ViewModelState)GetValue(StateProperty); }
			set { SetValue(StateProperty, value); }
		}
		public static readonly DependencyProperty StateProperty = DependencyProperty.Register(nameof(State), typeof(ViewModelState), typeof(AddHumanDialogViewModel));

		#endregion

		#region Commands

		private void InitCommands()
		{
			ViewLoaded = new Command(OnViewLoadedExecute);
			BindHumenGrid = new Command<Grid>(OnBindHumenGridExecute);
		}

		public Command<Grid> BindHumenGrid { get; private set; }
		private void OnBindHumenGridExecute(Grid humenGrid)
		{
			if (this.humenGrid == null)
			{
				this.humenGrid = humenGrid;
			}
		}

		public Command ViewLoaded { get; private set; }
		private async void OnViewLoadedExecute()
		{
			if(State == ViewModelState.Initialized)
			{
				State = ViewModelState.Loading;
				foreach (HumanData humanData in model.LoadHumans().OrderBy(x => x.Name))
				{
					new Item(humanData).AddToGrid(humenGrid);
					await Task.Delay(1);
				}
				State = ViewModelState.Loaded;
			}
		}

		#endregion

		#region Methods

		#endregion

		class Item
		{
			static readonly Style checkBoxItemStyle = Application.Current.FindResource("AddHumanDialogItemCheckBox") as Style;
			static readonly Style textBlockItemStyle = Application.Current.FindResource("AddHumanDialogItemTextBlock") as Style;

			public Item(HumanData dataContext)
			{
				NameCheckBox = new CheckBox { Content = dataContext.Name, Style = checkBoxItemStyle };
				TemplateTextBlock = new TextBlock { Text = dataContext.DefaultTemplate, Style = textBlockItemStyle };
				Grid.SetColumn(TemplateTextBlock, 1);
				DataContext = dataContext;
			}

			public CheckBox NameCheckBox { get; private set; }
			public TextBlock TemplateTextBlock { get; private set; }
			public HumanData DataContext { get; set; }

			public void AddToGrid(Grid grid)
			{
				Grid.SetRow(NameCheckBox, grid.RowDefinitions.Count);
				Grid.SetRow(TemplateTextBlock, grid.RowDefinitions.Count);
				grid.RowDefinitions.Add(new RowDefinition());
				grid.Children.Add(NameCheckBox);
				grid.Children.Add(TemplateTextBlock);
			}
		}
	}
}
