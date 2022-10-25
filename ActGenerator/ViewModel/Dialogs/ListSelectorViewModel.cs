using MaterialDesignThemes.Wpf;
using Mvvm;
using Mvvm.Commands;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ActGenerator.ViewModel.Dialogs
{
	class ListSelectorViewModel : DependencyObject
	{
		private ObservableRangeCollection<object> items = new ObservableRangeCollection<object>();

		public ListSelectorViewModel()
		{
			InitCommands();
		}

		#region Properties

		public INotifyCollectionChanged Items => items;

		public string ItemsDisplayMemberPath
		{
			get { return (string)GetValue(ItemsDisplayMemberPathProperty); }
			set { SetValue(ItemsDisplayMemberPathProperty, value); }
		}
		public static readonly DependencyProperty ItemsDisplayMemberPathProperty = DependencyProperty.Register(nameof(ItemsDisplayMemberPath), typeof(string), typeof(ListSelectorViewModel));

		public IList SelectedItems { get; private set; } = null;

		public bool IsAddingPressed { get; private set; } = false;

		#endregion

		#region Commands

		private void InitCommands()
		{
			AddCommand = new Command<IList>(OnAddCommandExecute);
			UnselectAllObjectsList = new Command<ListView>(OnUnselectAllObjectsListExecute);
		}

		public Command<IList> AddCommand { get; private set; }
		private void OnAddCommandExecute(IList selectedItems)
		{
			DialogHost.CloseDialogCommand.Execute(null, null);
			SelectedItems = selectedItems;
			IsAddingPressed = true;
		}

		public Command<ListView> UnselectAllObjectsList { get; private set; }
		private void OnUnselectAllObjectsListExecute(ListView listView)
		{
			listView.UnselectAll();
			IsAddingPressed = false;
		}

		#endregion

		#region Methods

		public void SetItems(IEnumerable items)
		{
			this.items.SuppressingNotifications = true;
			this.items.Clear();
			this.items.SuppressingNotifications = false;
			this.items.AddRange(items.Cast<object>());
		}

		#endregion
	}
}
