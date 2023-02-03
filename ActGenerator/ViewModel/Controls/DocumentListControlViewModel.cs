using ActGenerator.Model;
using ActGenerator.View.Controls;
using Dml;
using DocumentMakerModelLibrary.Files;
using Mvvm.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace ActGenerator.ViewModel.Controls
{
	public class DocumentListControlViewModel : DependencyObject
	{
		UIElementCollection itemsCollection = null;

		OpenFileDialog openFileDialog = new OpenFileDialog { Multiselect = true, Filter = "Файли повного акту(*" + DcmkFile.Extension + ") | *" + DcmkFile.Extension };

		public DocumentListControlViewModel()
		{
			InitCommands();
		}

		#region Properties

		public ObservableRangeCollection<DateTimeItem> DateTimeItems { get; private set; } = new ObservableRangeCollection<DateTimeItem>()
		{
			new DateTimeItem{ Text = "———", DateTime = new DateTime(1, 1, 1) },
			new DateTimeItem{ Text = "тиждня", DateTime = new DateTime(1, 1, 8) },
			new DateTimeItem{ Text = "місяця", DateTime = new DateTime(1, 2, 1) },
			new DateTimeItem{ Text = "3 місяців", DateTime = new DateTime(1, 4, 1) },
			new DateTimeItem{ Text = "пів року", DateTime = new DateTime(1, 7, 1) },
			new DateTimeItem{ Text = "року", DateTime = new DateTime(2, 1, 1) },
			new DateTimeItem{ Text = "всього часу", DateTime = new DateTime(100, 1, 1) },
		};

		public DateTimeItem SelectedDateTimeItem
		{
			get { return (DateTimeItem)GetValue(SelectedDateTimeItemProperty); }
			set { SetValue(SelectedDateTimeItemProperty, value); }
		}
		public static readonly DependencyProperty SelectedDateTimeItemProperty = DependencyProperty.Register(nameof(SelectedDateTimeItem), typeof(DateTimeItem), typeof(DocumentListControlViewModel));

		#endregion

		#region Commands

		private void InitCommands()
		{
			BindItemsCollection = new Command<UIElementCollection>(OnBindItemsCollectionExecute);
			AddAct = new Command(OnAddActExecute);
			RemoveAct = new Command<DocumentListItemControl>(OnRemoveActExecute);
		}

		public Command<UIElementCollection> BindItemsCollection { get; private set; }
		private void OnBindItemsCollectionExecute(UIElementCollection itemsCollection)
		{
			if (this.itemsCollection == null)
			{
				this.itemsCollection = itemsCollection;
			}
		}

		public Command AddAct { get; private set; }
		private async void OnAddActExecute()
		{
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				foreach(string filename in openFileDialog.FileNames)
				{
					LoadAct(filename);
					await Task.Delay(1);
				}
				CollectionChangedCommand?.Execute();
			}
		}

		private Command<DocumentListItemControl> RemoveAct { get; set; }
		private void OnRemoveActExecute(DocumentListItemControl documentListItemControl)
		{
			itemsCollection.Remove(documentListItemControl);
			CollectionChangedCommand?.Execute();
		}

		public Command CollectionChangedCommand { get; set; }

		#endregion

		#region Methods

		public List<DcmkFile> GetDocumentList()
		{
			return itemsCollection
				.Cast<DocumentListItemControl>()
				.Select(x => (DcmkFile)x.DataContext)
				.ToList();
		}

		public void LoadAct(string path)
		{
			if (itemsCollection.Cast<DocumentListItemControl>().FirstOrDefault(x => x.FullName == path) == null)
			{
				DocumentListItemControl documentListItemControl = new DocumentListItemControl
				{
					FullName = path,
					FileName = Path.GetFileName(path),
				};
				documentListItemControl.RemoveCommand = new Command(() => RemoveAct.Execute(documentListItemControl));

				DcmkFile context = new DcmkFile(path);
				context.Load();
				documentListItemControl.DataContext = context;

				itemsCollection.Add(documentListItemControl);
			}
		}

		#endregion
	}
}
