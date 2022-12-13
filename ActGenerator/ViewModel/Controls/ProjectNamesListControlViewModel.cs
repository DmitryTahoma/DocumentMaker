﻿using ActGenerator.Model.Controls;
using ActGenerator.View.Dialogs;
using ActGenerator.ViewModel.Dialogs;
using ActGenerator.ViewModel.Interfaces;
using DocumentMaker.Security;
using MaterialDesignThemes.Wpf;
using Mvvm;
using Mvvm.Commands;
using ProjectEditorLib.ViewModel;
using ProjectsDb.Context;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ActGenerator.ViewModel.Controls
{
	public class ProjectNamesListControlViewModel : DependencyObject, IContainDialogHostId, ICryptedConnectionStringRequired
	{
		ProjectNamesListControlModel model = new ProjectNamesListControlModel();

		readonly Style itemStyle = Application.Current.FindResource("DeletableMaterialDesignOutlineChip") as Style;

		UIElementCollection projectCollection = null;

		AddProjectNameDialog addProjectNameDialog = null;
		AddProjectNameDialogViewModel addProjectNameDialogViewModel = null;

		public ProjectNamesListControlViewModel()
		{
			addProjectNameDialog = new AddProjectNameDialog();
			addProjectNameDialogViewModel = (AddProjectNameDialogViewModel)addProjectNameDialog.DataContext;

			InitCommands();

			State = ViewModelState.Initialized;
		}

		#region Properties

		public string DialogHostId { get; set; } = null;

		public ViewModelState State
		{
			get { return (ViewModelState)GetValue(StateProperty); }
			set { SetValue(StateProperty, value); }
		}
		public static readonly DependencyProperty StateProperty = DependencyProperty.Register(nameof(State), typeof(ViewModelState), typeof(ProjectNamesListControlViewModel));

		#endregion

		#region Commands

		private void InitCommands()
		{
			OpenAddProjectNameDialog = new Command(OnOpenAddProjectNameDialogExecute);
			BindProjectCollection = new Command<UIElementCollection>(OnBindProjectCollectionExecute);
			RemoveChip = new Command<Chip>(OnRemoveChipExecute);
			ViewLoaded = new Command(OnViewLoadedExecute);
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

		public Command ViewLoaded { get; private set; }
		private async void OnViewLoadedExecute()
		{
			if(State == ViewModelState.Initialized)
			{
				State = ViewModelState.Loaded;
			}
			else if(State == ViewModelState.Loaded)
			{
				State = ViewModelState.Loading;
				if(projectCollection.Count > 0)
				{
					bool needLoadProjects = projectCollection.Cast<Chip>().FirstOrDefault(x => x.DataContext is Project) != null;
					bool needLoadAlternativeName = projectCollection.Cast<Chip>().FirstOrDefault(x => x.DataContext is AlternativeProjectName) != null;
					List<Project> projects = null;
					List<AlternativeProjectName> alternativeProjectNames = null;

					await model.ConnectDbAsync();
					if (needLoadProjects) projects = await model.LoadProjectsAsync();
					if (needLoadAlternativeName) alternativeProjectNames = await model.LoadAlternativeProjectNamesAsync();
					await model.DisconnectDbAsync();

					List<IDbObject> loadedObjects = new List<IDbObject>();
					if (needLoadProjects) loadedObjects.AddRange(projects);
					if (needLoadAlternativeName) loadedObjects.AddRange(alternativeProjectNames);

					foreach(Chip chip in projectCollection.Cast<Chip>().ToList())
					{
						IDbObject context = chip.DataContext as IDbObject;
						IDbObject loadedContext = loadedObjects.FirstOrDefault(x => x.GetType() == context.GetType() && x.Id == context.Id);
						if(loadedContext != null)
						{
							chip.DataContext = context;
							chip.Content = context.ToString();
						}
						else
						{
							projectCollection.Remove(chip);
						}
					}
				}
				State = ViewModelState.Loaded;
			}
		}

		#endregion

		#region Methods

		public void SetCryptedConnectionString(CryptedConnectionString cryptedConnectionString)
		{
			model.ConnectionString = cryptedConnectionString;
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
