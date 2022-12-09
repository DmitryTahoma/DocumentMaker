using ActGenerator.View.Dialogs;
using ActGenerator.ViewModel.Dialogs;
using DocumentMaker.Security;
using MaterialDesignThemes.Wpf;
using Mvvm.Commands;
using ProjectEditorLib.ViewModel;
using ProjectsDb.Context;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ActGenerator.ViewModel.Controls
{
	public class ProjectNamesListControlViewModel : ICryptedConnectionStringRequired
	{
		readonly Style itemStyle = Application.Current.FindResource("DeletableMaterialDesignOutlineChip") as Style;

		UIElementCollection projectCollection = null;

		AddProjectNameDialog addProjectNameDialog = null;
		AddProjectNameDialogViewModel addProjectNameDialogViewModel = null;

		public ProjectNamesListControlViewModel()
		{
			addProjectNameDialog = new AddProjectNameDialog();
			addProjectNameDialogViewModel = (AddProjectNameDialogViewModel)addProjectNameDialog.DataContext;

			InitCommands();
		}

		#region Properties

		public string DialogHostId { get; set; } = null;

		#endregion

		#region Commands

		private void InitCommands()
		{
			OpenAddProjectNameDialog = new Command(OnOpenAddProjectNameDialogExecute);
			BindProjectCollection = new Command<UIElementCollection>(OnBindProjectCollectionExecute);
			RemoveChip = new Command<Chip>(OnRemoveChipExecute);
		}

		public Command OpenAddProjectNameDialog { get; private set; }
		private async void OnOpenAddProjectNameDialogExecute()
		{
			await DialogHost.Show(addProjectNameDialog, DialogHostId);
			if (addProjectNameDialogViewModel.IsPressedAdd)
			{
				List<IDbObject> selectedItems = addProjectNameDialogViewModel.SelectedProjectsAndNames.ToList();

				List<Chip> castedProjectCollection = projectCollection.Cast<Chip>().ToList();
				foreach(IDbObject selectedItem in selectedItems)
				{
					if(null == castedProjectCollection.FirstOrDefault(x => x.DataContext.GetType() == selectedItem.GetType() && ((IDbObject)x.DataContext).Id == selectedItem.Id))
					{
						projectCollection.Add(CreateProjectChip(selectedItem));
					}
				}
			}
		}

		public Command<UIElementCollection> BindProjectCollection { get; private set; }
		private void OnBindProjectCollectionExecute(UIElementCollection projectCollection)
		{
			if(this.projectCollection == null)
			{
				this.projectCollection = projectCollection;
			}
		}

		public Command<Chip> RemoveChip { get; private set; }
		private void OnRemoveChipExecute(Chip chip)
		{
			projectCollection.Remove(chip);
		}

		#endregion

		#region Methods

		public void SetCryptedConnectionString(CryptedConnectionString cryptedConnectionString)
		{
			addProjectNameDialogViewModel.SetCryptedConnectionString(cryptedConnectionString);
		}

		private Chip CreateProjectChip(IDbObject context)
		{
			Chip chip = new Chip
			{
				DataContext = context,
				Style = itemStyle,
				Content = context.ToString(),
			};
			chip.DeleteCommand = new Command(() => RemoveChip.Execute(chip));

			return chip;
		}

		#endregion
	}
}
