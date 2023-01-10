using ActGenerator.View.Controls;
using DocumentMakerModelLibrary.Files;
using Mvvm.Commands;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;

namespace ActGenerator.ViewModel.Controls
{
	public class DocumentListControlViewModel
	{
		UIElementCollection itemsCollection = null;

		OpenFileDialog openFileDialog = new OpenFileDialog { Multiselect = true, Filter = "Файли повного акту(*" + DcmkFile.Extension + ") | *" + DcmkFile.Extension };

		public DocumentListControlViewModel()
		{
			InitCommands();
		}

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
					if(itemsCollection.Cast<DocumentListItemControl>().FirstOrDefault(x => x.FullName == filename) == null)
					{
						DocumentListItemControl documentListItemControl = new DocumentListItemControl
						{
							FullName = filename,
							FileName = Path.GetFileName(filename),
						};
						documentListItemControl.RemoveCommand = new Command(() => RemoveAct.Execute(documentListItemControl));

						DcmkFile context = new DcmkFile(filename);
						context.Load();
						documentListItemControl.DataContext = context;

						itemsCollection.Add(documentListItemControl);
					}
					await Task.Delay(1);
				}
			}
		}

		private Command<DocumentListItemControl> RemoveAct { get; set; }
		private void OnRemoveActExecute(DocumentListItemControl documentListItemControl)
		{
			itemsCollection.Remove(documentListItemControl);
		}

		#endregion

		#region Methods

		public List<DcmkFile> GetDocumentList()
		{
			return itemsCollection
				.Cast<DocumentListItemControl>()
				.Select(x => (DcmkFile)x.DataContext)
				.ToList();
		}

		#endregion
	}
}
