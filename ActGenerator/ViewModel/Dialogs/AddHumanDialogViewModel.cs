using ActGenerator.Model.Dialogs;
using ActGenerator.View.Controls;
using DocumentMakerModelLibrary.OfficeFiles.Human;
using MaterialDesignThemes.Wpf;
using Mvvm;
using Mvvm.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ActGenerator.ViewModel.Dialogs
{
	public class AddHumanDialogViewModel : DependencyObject
	{
		AddHumanDialogModel model = new AddHumanDialogModel();

		List<Item> items = new List<Item>();
		bool needUpdateHumenCheckBoxIsChecked = true;

		Grid humenGrid = null;

		public AddHumanDialogViewModel()
		{
			InitCommands();
			SelectedHumanData = items
				.Where(x => x.NameCheckBox.IsChecked == true)
				.Select(y => y.DataContext);

			State = ViewModelState.Initialized;
		}

		#region Properties

		public ViewModelState State
		{
			get { return (ViewModelState)GetValue(StateProperty); }
			set { SetValue(StateProperty, value); }
		}
		public static readonly DependencyProperty StateProperty = DependencyProperty.Register(nameof(State), typeof(ViewModelState), typeof(AddHumanDialogViewModel));

		public bool? HumenCheckBoxIsChecked
		{
			get { return (bool?)GetValue(HumenCheckBoxIsCheckedProperty); }
			set { SetValue(HumenCheckBoxIsCheckedProperty, value); }
		}
		public static readonly DependencyProperty HumenCheckBoxIsCheckedProperty = DependencyProperty.Register(nameof(HumenCheckBoxIsChecked), typeof(bool?), typeof(AddHumanDialogViewModel), new PropertyMetadata(false));

		public bool IsPressedAdd { get; private set; } = false;

		public IEnumerable<HumanData> SelectedHumanData { get; private set; } = null;

		#endregion

		#region Commands

		private void InitCommands()
		{
			ViewLoaded = new Command(OnViewLoadedExecute);
			BindHumenGrid = new Command<Grid>(OnBindHumenGridExecute);
			ChangeIsCheckedAllHumen = new Command(OnChangeIsCheckedAllHumenExecute);
			AddCommand = new Command(OnAddCommandExecute);
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
			HumenCheckBoxIsChecked = false;
			ChangeIsCheckedAllHumen?.Execute();
			IsPressedAdd = false;

			if (State == ViewModelState.Initialized)
			{
				State = ViewModelState.Loading;
				foreach (HumanData humanData in model.LoadHumans().OrderBy(x => x.Name))
				{
					Item item = new Item(humanData);
					item.NameCheckBox.Checked += NameCheckBoxCheckedChanged;
					item.NameCheckBox.Unchecked += NameCheckBoxCheckedChanged;
					item.AddToGrid(humenGrid);
					items.Add(item);
					await Task.Delay(1);
				}
				UpdateHumenCheckBoxIsChecked();
				State = ViewModelState.Loaded;
			}
		}

		public Command ChangeIsCheckedAllHumen { get; private set; }
		private void OnChangeIsCheckedAllHumenExecute()
		{
			needUpdateHumenCheckBoxIsChecked = false;
			foreach(Item item in items)
			{
				item.NameCheckBox.IsChecked = HumenCheckBoxIsChecked;
			}
			needUpdateHumenCheckBoxIsChecked = true;
		}

		public Command AddCommand { get; private set; }
		private void OnAddCommandExecute()
		{
			IsPressedAdd = true;

			DialogHost.CloseDialogCommand.Execute(null, null);
		}

		#endregion

		#region Methods

		private void UpdateHumenCheckBoxIsChecked()
		{
			if (!needUpdateHumenCheckBoxIsChecked) return;

			HumenCheckBoxIsChecked = CheckBoxList.UpdateGeneralCheckBoxState(items.Select(x => x.NameCheckBox));
		}

		private void NameCheckBoxCheckedChanged(object sender, RoutedEventArgs e)
		{
			UpdateHumenCheckBoxIsChecked();
		}

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
