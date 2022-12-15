using ActGenerator.Model.Controls;
using ActGenerator.View.Controls;
using ActGenerator.View.Dialogs;
using ActGenerator.ViewModel.Dialogs;
using ActGenerator.ViewModel.Interfaces;
using DocumentMakerModelLibrary;
using DocumentMakerModelLibrary.OfficeFiles.Human;
using MaterialDesignThemes.Wpf;
using Mvvm.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ActGenerator.ViewModel.Controls
{
	public class HumenListControlViewModel : DependencyObject, IContainDialogHostId
	{
		HumenListControlModel model = new HumenListControlModel();

		AddHumanDialog addHumanDialog = new AddHumanDialog();
		AddHumanDialogViewModel addHumanDialogViewModel = null;

		ScrollViewer humenHeaderScrollViewer = null;
		Grid humenGrid = null;
		List<Item> items = new List<Item>();

		bool needUpdateHumenCheckBoxIsChecked = true;

		public HumenListControlViewModel()
		{
			addHumanDialogViewModel = addHumanDialog.DataContext as AddHumanDialogViewModel;

			InitCommands();
		}

		#region Properties

		public string DialogHostId { get; set; } = null;

		public double ScrollViewerHorizontalOffset
		{
			get { return (double)GetValue(ScrollViewerHorizontalOffsetProperty); }
			set { SetValue(ScrollViewerHorizontalOffsetProperty, value); }
		}
		public static readonly DependencyProperty ScrollViewerHorizontalOffsetProperty = DependencyProperty.Register(nameof(ScrollViewerHorizontalOffset), typeof(double), typeof(HumenListControlViewModel));

		public double ScrollViewerVerticalOffset
		{
			get { return (double)GetValue(ScrollViewerVerticalOffsetProperty); }
			set { SetValue(ScrollViewerVerticalOffsetProperty, value); }
		}
		public static readonly DependencyProperty ScrollViewerVerticalOffsetProperty = DependencyProperty.Register(nameof(ScrollViewerVerticalOffset), typeof(double), typeof(HumenListControlViewModel));

		public bool? HumenCheckBoxIsChecked
		{
			get { return (bool?)GetValue(HumenCheckBoxIsCheckedProperty); }
			set { SetValue(HumenCheckBoxIsCheckedProperty, value); }
		}
		public static readonly DependencyProperty HumenCheckBoxIsCheckedProperty = DependencyProperty.Register(nameof(HumenCheckBoxIsChecked), typeof(bool?), typeof(HumenListControlViewModel), new PropertyMetadata(false));

		#endregion

		#region Commands

		private void InitCommands()
		{
			AddHumanCommand = new Command(OnAddHumanCommandExecute);
			BindHumenGrid = new Command<Grid>(OnBindHumenGridExecute);
			BindHumenHeaderScrollViewer = new Command<ScrollViewer>(OnBindHumenHeaderScrollViewerExecute);
			SetScrollChanges = new Command<ScrollChangedEventArgs>(OnSetScrollChangesExecute);
			ChangeIsCheckedAllHumen = new Command(OnChangeIsCheckedAllHumenExecute);
		}

		public Command AddHumanCommand { get; private set; }
		private async void OnAddHumanCommandExecute()
		{
			await DialogHost.Show(addHumanDialog, DialogHostId);
			if(addHumanDialogViewModel.IsPressedAdd)
			{
				foreach (HumanData humanData in addHumanDialogViewModel.SelectedHumanData)
				{
					if(items.FirstOrDefault(x => x.DataContext == humanData) == null)
					{
						items.Add(CreateItem(humanData));
						UpdateHumenCheckBoxIsChecked();
						await Task.Delay(1);
					}
				}
			}
		}

		public Command<Grid> BindHumenGrid { get; private set; }
		private void OnBindHumenGridExecute(Grid humenGrid)
		{
			if(this.humenGrid == null)
			{
				this.humenGrid = humenGrid;
			}
		}

		public Command<ScrollViewer> BindHumenHeaderScrollViewer { get; private set; }
		private void OnBindHumenHeaderScrollViewerExecute(ScrollViewer humenHeaderScrollViewer)
		{
			if (this.humenHeaderScrollViewer == null)
			{
				this.humenHeaderScrollViewer = humenHeaderScrollViewer;
			}
		}

		public Command<ScrollChangedEventArgs> SetScrollChanges { get; private set; }
		private void OnSetScrollChangesExecute(ScrollChangedEventArgs e)
		{
			if (humenHeaderScrollViewer != null)
			{
				humenHeaderScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);
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

		#endregion

		#region Methods

		private Item CreateItem(HumanData humanData)
		{
			Item item = new Item(humanData);
			List<FullDocumentTemplate> itemsSource = model.DocumentTemplatesList.ToList();
			FullDocumentTemplate deafultTemplate = itemsSource.FirstOrDefault(x => x.Name == humanData.DefaultTemplate);
			if (deafultTemplate != null)
			{
				itemsSource.Remove(deafultTemplate);
				itemsSource.Insert(0, deafultTemplate);
			}
			item.TemplateCheckBoxList.ItemsSource = itemsSource;
			if (deafultTemplate != null)
			{
				item.TemplateCheckBoxList.CheckFirst();
			}
			item.NameCheckBox.Checked += NameCheckBoxCheckedChanged;
			item.NameCheckBox.Unchecked += NameCheckBoxCheckedChanged;
			item.AddToGrid(humenGrid);
			return item;
		}

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
			static readonly Style textBoxItemStyle = Application.Current.FindResource("HumenListControlItemTextBox") as Style;

			public Item(HumanData dataContext)
			{
				NameCheckBox = new CheckBox { Content = dataContext.Name };
				SumTextBox = new TextBox { Style = textBoxItemStyle };
				Grid.SetColumn(SumTextBox, 1);
				TemplateCheckBoxList = new CheckBoxList();
				Grid.SetColumn(TemplateCheckBoxList, 2);
				DataContext = dataContext;
			}

			public CheckBox NameCheckBox { get; private set; }
			public TextBox SumTextBox { get; private set; }
			public CheckBoxList TemplateCheckBoxList { get; private set; }
			public HumanData DataContext { get; set; }

			public void AddToGrid(Grid grid)
			{
				Grid.SetRow(NameCheckBox, grid.RowDefinitions.Count);
				Grid.SetRow(SumTextBox, grid.RowDefinitions.Count);
				Grid.SetRow(TemplateCheckBoxList, grid.RowDefinitions.Count);
				grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
				grid.Children.Add(NameCheckBox);
				grid.Children.Add(SumTextBox);
				grid.Children.Add(TemplateCheckBoxList);
			}
		}
	}
}
