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

		UIElementCollection humenStackCollection = null;
		IEnumerable<HumenListItemControl> items = null;
		IEnumerable<HumenListItemControlViewModel> itemsViewModel = null;

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
			BindHumenStack = new Command<UIElementCollection>(OnBindHumenStackExecute);
			BindHumenHeaderScrollViewer = new Command<ScrollViewer>(OnBindHumenHeaderScrollViewerExecute);
			SetScrollChanges = new Command<ScrollChangedEventArgs>(OnSetScrollChangesExecute);
			ChangeIsCheckedAllHumen = new Command(OnChangeIsCheckedAllHumenExecute);
			RemoveSelectedHumen = new Command(OnRemoveSelectedHumenExecute);
		}

		public Command AddHumanCommand { get; private set; }
		private async void OnAddHumanCommandExecute()
		{
			await DialogHost.Show(addHumanDialog, DialogHostId);
			if(addHumanDialogViewModel.IsPressedAdd)
			{
				foreach (HumanData humanData in addHumanDialogViewModel.SelectedHumanData)
				{
					if(items.FirstOrDefault(x => ((HumenListItemControlViewModel)x.DataContext).Model == humanData) == null)
					{
						AddItem(humanData);
						UpdateHumenCheckBoxIsChecked();
						await Task.Delay(1);
					}
				}
			}
		}

		public Command<UIElementCollection> BindHumenStack { get; private set; }
		private void OnBindHumenStackExecute(UIElementCollection humenStackCollection)
		{
			if(this.humenStackCollection == null)
			{
				this.humenStackCollection = humenStackCollection;
				items = this.humenStackCollection.Cast<HumenListItemControl>();
				itemsViewModel = items.Select(x => (HumenListItemControlViewModel)x.DataContext);
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
			foreach(HumenListItemControlViewModel item in itemsViewModel)
			{
				item.IsChecked = HumenCheckBoxIsChecked.HasValue && HumenCheckBoxIsChecked.Value;
			}
			needUpdateHumenCheckBoxIsChecked = true;
		}

		public Command RemoveSelectedHumen { get; private set; }
		private async void OnRemoveSelectedHumenExecute()
		{
			List<HumenListItemControl> removeList = new List<HumenListItemControl>();
			foreach (HumenListItemControl item in items)
			{
				if (item.CheckBox.IsChecked == true)
				{
					removeList.Add(item);
				}
			}

			foreach(HumenListItemControl removeItem in removeList)
			{
				humenStackCollection.Remove(removeItem);
				UpdateHumenCheckBoxIsChecked();
				await Task.Delay(1);
			}
			HumenCheckBoxIsChecked = false;
		}

		#endregion

		#region Methods

		private void AddItem(HumanData humanData)
		{
			HumenListItemControl item = new HumenListItemControl();
			HumenListItemControlViewModel itemViewModel = (HumenListItemControlViewModel)item.DataContext;
			itemViewModel.Model = humanData;
			List<FullDocumentTemplate> itemsSource = model.DocumentTemplatesList.ToList();
			FullDocumentTemplate deafultTemplate = itemsSource.FirstOrDefault(x => x.Name == humanData.DefaultTemplate);
			if (deafultTemplate != null)
			{
				itemsSource.Remove(deafultTemplate);
				itemsSource.Insert(0, deafultTemplate);
			}
			itemViewModel.DocumentTemplatesList = itemsSource;
			if (deafultTemplate != null)
			{
				item.CheckBoxList.CheckFirst();
			}
			item.CheckBox.Checked += NameCheckBoxCheckedChanged;
			item.CheckBox.Unchecked += NameCheckBoxCheckedChanged;
			humenStackCollection.Add(item);
		}

		private void UpdateHumenCheckBoxIsChecked()
		{
			if (!needUpdateHumenCheckBoxIsChecked) return;

			HumenCheckBoxIsChecked = CheckBoxList.UpdateGeneralCheckBoxState(items.Select(x => x.CheckBox));
		}

		private void NameCheckBoxCheckedChanged(object sender, RoutedEventArgs e)
		{
			UpdateHumenCheckBoxIsChecked();
		}

		#endregion
	}
}
