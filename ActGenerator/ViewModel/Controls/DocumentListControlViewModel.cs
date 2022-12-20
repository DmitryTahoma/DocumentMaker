using ActGenerator.View.Controls;
using DocumentMakerModelLibrary.Files;
using Mvvm.Commands;
using System.IO;
using System.Linq;
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
		private void OnAddActExecute()
		{
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				foreach(string filename in openFileDialog.FileNames)
				{
					if(itemsCollection.Cast<DocumentListItemControl>().FirstOrDefault(x => x.FullName == filename) == null)
					{
						itemsCollection.Add(new DocumentListItemControl
						{
							FullName = filename,
							FileName = Path.GetFileName(filename),
						});
					}
				}
			}
		}

		#endregion
	}
}
