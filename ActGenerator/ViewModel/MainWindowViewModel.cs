using ActGenerator.Model;
using Dml.Controller.Validation;
using MaterialDesignThemes.Wpf;
using Mvvm;
using Mvvm.Commands;
using ProjectEditorLib.View.Dialogs;
using ProjectEditorLib.ViewModel;
using ProjectEditorLib.ViewModel.Controls;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace ActGenerator.ViewModel
{
	class MainWindowViewModel : DependencyObject
	{
		ActGeneratorSession model = new ActGeneratorSession();
		ActGeneratorViewModel actGeneratorViewModel = null;
		ProjectEditViewModel projectEditViewModel = null;
		ProjectRestoreViewModel projectRestoreViewModel = null;

		SelectProjectDialog selectProjectDialog = new SelectProjectDialog();
		CreateProjectDialog createProjectDialog = new CreateProjectDialog();

		public MainWindowViewModel()
		{
			InitCommands();

			((SelectProjectDialogViewModel)selectProjectDialog.DataContext).DialogHostId = DialogHostId;
			((CreateProjectDialogViewModel)createProjectDialog.DataContext).DialogHostId = DialogHostId;
		}

		#region Properties

		public int SelectedTabIndex
		{
			get { return (int)GetValue(SelectedTabIndexProperty); }
			set { SetValue(SelectedTabIndexProperty, value); }
		}
		public static readonly DependencyProperty SelectedTabIndexProperty = DependencyProperty.Register(nameof(SelectedTabIndex), typeof(int), typeof(MainWindowViewModel));

		public string DialogHostId => "TopDialogHost";

		#endregion

		#region Commands

		private void InitCommands()
		{
			LoadSession = new Command<MainWindow>(OnLoadSessionExecute);
			SaveSession = new Command<MainWindow>(OnSaveSessionExecute);
			BindActGenerator = new Command<ActGeneratorViewModel>(OnBindActGeneratorExecute);
			ShowGeneratorTab = new Command(OnShowGeneratorTabExecute, CanChangeTabIndex);
			OpenProject = new Command(OnOpenProjectExecute, CanChangeTabIndex);
			CreateProject = new Command(OnCreateProjectExecute, CanChangeTabIndex);
			BindProjectEditor = new Command<ProjectEditViewModel>(OnBindProjectEditorExecute);
			RestoreProject = new Command(OnRestoreProjectExecute, CanChangeTabIndex);
			BlockTabControlHotKeys = new Command<KeyEventArgs>(OnBlockTabControlHotKeysExecute);
			CheckHaveUnsavedChanges = new Command<CancelEventArgs>(OnCheckHaveUnsavedChangesExecute);
			ClearKeyboardFocus = new Command(OnClearKeyboardFocusExecute);
			ClearKeyboardFocusOnEnter = new Command<KeyEventArgs>(OnClearKeyboardFocusOnEnterExecute);
			SendCryptedConnectionString = new Command<ICryptedConnectionStringRequired>(OnSendCryptedConnectionStringExecute);
			SetInvariantNumberFormat = new Command(OnSetInvariantNumberFormatExecute);
			BindProjectRestore = new Command<ProjectRestoreViewModel>(OnBindProjectRestoreExecute);
		}

		public Command<MainWindow> LoadSession { get; private set; }
		private void OnLoadSessionExecute(MainWindow window)
		{
			try
			{
				model.Load();
			}
			catch(UnauthorizedAccessException)
			{
				model.ReopenApplicationElevatedRights();
				return;
			}

			window.Height = model.WindowHeight;
			window.Width = model.WindowWidth;

			if(model.WindowTop == -1 || model.WindowLeft == -1)
			{
				WindowValidator.MoveToPrimaryScreenCenterPosition(window);
			}
			else
			{
				window.Top = model.WindowTop;
				window.Left = model.WindowLeft;
			}
			WindowValidator.MoveToValidPosition(window);

			window.WindowState = model.WindowState;
			window.Height = model.WindowHeight;
			window.Width = model.WindowWidth;

			actGeneratorViewModel.LoadFromSession(model);

			((SelectProjectDialogViewModel)selectProjectDialog.DataContext).SetCryptedConnectionString(model.CryptedConnectionString);
		}

		public Command<MainWindow> SaveSession { get; private set; }
		private void OnSaveSessionExecute(MainWindow window)
		{
			if (!model.ApplicationLoaded) return;

			model.WindowTop = window.Top;
			model.WindowLeft = window.Left;
			model.WindowHeight = window.Height;
			model.WindowWidth = window.Width;
			model.WindowState = window.WindowState == WindowState.Minimized ? WindowState.Normal : window.WindowState;

			model.MinSumText = actGeneratorViewModel.MinSumText;
			model.MaxSumText = actGeneratorViewModel.MaxSumText;
			model.NotUseOldWorks = actGeneratorViewModel.NotUseOldWorks;
			model.SelectedDateTimeItem = actGeneratorViewModel.SelectedDateTimeItem;
			model.LastOpenedProjectName = ((SelectProjectDialogViewModel)selectProjectDialog.DataContext).LastOpenedProjectName;

			model.Save();
		}

		public Command<ActGeneratorViewModel> BindActGenerator { get; private set; }
		private void OnBindActGeneratorExecute(ActGeneratorViewModel actGenerator)
		{
			actGeneratorViewModel = actGenerator;
		}

		public Command ShowGeneratorTab { get; private set; }
		private void OnShowGeneratorTabExecute()
		{
			if (BeforeSelectedTabIndexChanged()) return;

			SelectedTabIndex = 0;
		}

		public Command OpenProject { get; private set; }
		private async void OnOpenProjectExecute()
		{
			if (BeforeSelectedTabIndexChanged()) return;

			SelectProjectDialogViewModel selectProjectDialogDataContext = (SelectProjectDialogViewModel)selectProjectDialog.DataContext;
			selectProjectDialogDataContext.LastOpenedProjectName = model.LastOpenedProjectName;
			if (SelectedTabIndex == 0) actGeneratorViewModel.CloseOpenedDialog.Execute();
			await DialogHost.Show(selectProjectDialog, DialogHostId);
			if(selectProjectDialogDataContext.IsOpen)
			{
				projectEditViewModel.State = ViewModelState.Loading;
				projectEditViewModel.SelectedEditProject = selectProjectDialogDataContext.SelectedProject;
				selectProjectDialogDataContext.LastOpenedProjectName = projectEditViewModel.SelectedEditProject.Name;
				SelectedTabIndex = 1;
				await projectEditViewModel.LoadProject();
			}
		}

		public Command CreateProject { get; private set; }
		private async void OnCreateProjectExecute()
		{
			if (BeforeSelectedTabIndexChanged()) return;

			if (SelectedTabIndex == 0) actGeneratorViewModel.CloseOpenedDialog.Execute();
			await DialogHost.Show(createProjectDialog, DialogHostId);
			if(createProjectDialog.DataContext is CreateProjectDialogViewModel createProjectDialogViewModel && createProjectDialogViewModel.IsCreate)
			{
				projectEditViewModel.State = ViewModelState.Initializing;
				SelectedTabIndex = 1;
				projectEditViewModel.CreationProjectName = createProjectDialogViewModel.ProjectName;
				((SelectProjectDialogViewModel)selectProjectDialog.DataContext).LastOpenedProjectName = projectEditViewModel.CreationProjectName;
				await projectEditViewModel.CreateProject();
				await projectEditViewModel.LoadProject();
			}
		}

		public Command<ProjectEditViewModel> BindProjectEditor { get; private set; }
		private void OnBindProjectEditorExecute(ProjectEditViewModel projectEdit)
		{
			if (!model.ApplicationLoaded) return;

			projectEditViewModel = projectEdit;
		}

		public Command RestoreProject { get; private set; }
		private async void OnRestoreProjectExecute()
		{
			if (BeforeSelectedTabIndexChanged()) return;

			if (SelectedTabIndex == 0) actGeneratorViewModel.CloseOpenedDialog.Execute();
			projectRestoreViewModel.State = ViewModelState.Loading;
			SelectedTabIndex = 2;
			await projectRestoreViewModel.LoadRemovedNodes();
		}

		public Command<KeyEventArgs> BlockTabControlHotKeys { get; private set; }
		private void OnBlockTabControlHotKeysExecute(KeyEventArgs e)
		{
			if ((Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && Keyboard.IsKeyDown(Key.Tab))
				|| (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) && Keyboard.IsKeyDown(Key.Tab)))
			{
				e.Handled = true;
			}
		}

		public Command<CancelEventArgs> CheckHaveUnsavedChanges { get; private set; }
		private void OnCheckHaveUnsavedChangesExecute(CancelEventArgs e)
		{
			if (SelectedTabIndex == 1 && !projectEditViewModel.CheckHaveUnsavedChangesAndSave(true))
				e.Cancel = true;
		}

		public Command ClearKeyboardFocus { get; private set; }
		private void OnClearKeyboardFocusExecute()
		{
			Keyboard.ClearFocus();
		}

		public Command<KeyEventArgs> ClearKeyboardFocusOnEnter { get; private set; }
		private void OnClearKeyboardFocusOnEnterExecute(KeyEventArgs e)
		{
			if(e.Key == Key.Enter)
			{ 
				Keyboard.ClearFocus();
			}
		}

		public Command<ICryptedConnectionStringRequired> SendCryptedConnectionString { get; private set; }
		private void OnSendCryptedConnectionStringExecute(ICryptedConnectionStringRequired connectionStringRequired)
		{
			if (!model.ApplicationLoaded) return;

			connectionStringRequired.SetCryptedConnectionString(model.CryptedConnectionString);
		}

		public Command SetInvariantNumberFormat { get; private set; }
		private void OnSetInvariantNumberFormatExecute()
		{
			if (!model.ApplicationLoaded) return;

			System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("uk-ua");
			culture.NumberFormat = System.Globalization.NumberFormatInfo.InvariantInfo;
			Thread.CurrentThread.CurrentCulture = culture;
			Thread.CurrentThread.CurrentUICulture = culture;
		}

		public Command<ProjectRestoreViewModel> BindProjectRestore { get; private set; }
		private void OnBindProjectRestoreExecute(ProjectRestoreViewModel projectRestoreViewModel)
		{
			this.projectRestoreViewModel = projectRestoreViewModel;
		}

		#endregion

		private bool BeforeSelectedTabIndexChanged()
		{
			if(SelectedTabIndex == 1)
			{
				model.LastOpenedProjectName = projectEditViewModel.SelectedEditProject.Name;
				return !projectEditViewModel.CheckHaveUnsavedChangesAndSave();
			}

			return false;
		}

		private bool CanChangeTabIndex(object obj)
		{
			return actGeneratorViewModel != null
				&& projectEditViewModel != null
				&& projectRestoreViewModel != null
				&& actGeneratorViewModel.State == ViewModelState.Loaded
				&& projectEditViewModel.State != ViewModelState.Initializing
				&& projectEditViewModel.State != ViewModelState.Loading
				&& !projectEditViewModel.IsLoadingBacks
				&& projectRestoreViewModel.State != ViewModelState.Loading
				&& !projectRestoreViewModel.IsLoadingBacks;
		}
	}
}
