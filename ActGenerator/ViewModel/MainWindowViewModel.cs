using ActGenerator.Model;
using Dml.Controller.Validation;
using MaterialDesignThemes.Wpf;
using Mvvm.Commands;
using ProjectEditorLib.View.Dialogs;
using ProjectEditorLib.ViewModel;
using ProjectEditorLib.ViewModel.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ActGenerator.ViewModel
{
	class MainWindowViewModel : DependencyObject
	{
		ActGeneratorSession model = new ActGeneratorSession();
		ActGeneratorViewModel actGeneratorViewModel = null;
		ProjectEditViewModel projectEditViewModel = null;

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
			ShowGeneratorTab = new Command(OnShowGeneratorTabExecute);
			OpenProject = new Command(OnOpenProjectExecute);
			CreateProject = new Command(OnCreateProjectExecute);
			BindProjectEditor = new Command<ProjectEditViewModel>(OnBindProjectEditorExecute);
			RestoreProject = new Command(OnRestoreProjectExecute);
			BlockTabControlHotKeys = new Command<KeyEventArgs>(OnBlockTabControlHotKeysExecute);
		}

		public Command<MainWindow> LoadSession { get; private set; }
		private void OnLoadSessionExecute(MainWindow window)
		{
			model.Load();
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
		}

		public Command<MainWindow> SaveSession { get; private set; }
		private void OnSaveSessionExecute(MainWindow window)
		{
			model.WindowTop = window.Top;
			model.WindowLeft = window.Left;
			model.WindowHeight = window.Height;
			model.WindowWidth = window.Width;
			model.WindowState = window.WindowState == WindowState.Minimized ? WindowState.Normal : window.WindowState;

			model.ProjectsList = new List<int>(actGeneratorViewModel.ProjectsList.Select(x => x.Id));
			model.HumanList = new List<ActGeneratorSession.HumanDataContextSave>(actGeneratorViewModel.HumanList.Select(x => new ActGeneratorSession.HumanDataContextSave(x)));
			model.MinSumText = actGeneratorViewModel.MinSumText;
			model.MaxSumText = actGeneratorViewModel.MaxSumText;
			model.IsUniqueNumbers = actGeneratorViewModel.IsUniqueNumbers;
			model.CanUseOldWorks = actGeneratorViewModel.CanUseOldWorks;
			model.SelectedDateTimeItem = actGeneratorViewModel.SelectedDateTimeItem;

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
			SelectedTabIndex = 0;
		}

		public Command OpenProject { get; private set; }
		private async void OnOpenProjectExecute()
		{
			actGeneratorViewModel.CloseOpenedDialog.Execute();
			await DialogHost.Show(selectProjectDialog, DialogHostId);
			if(selectProjectDialog.DataContext is SelectProjectDialogViewModel selectProjectDialogDataContext && selectProjectDialogDataContext.IsOpen)
			{
				projectEditViewModel.State = Mvvm.ViewModelState.Loading;
				projectEditViewModel.SelectedEditProject = selectProjectDialogDataContext.SelectedProject;
				SelectedTabIndex = 1;
				await projectEditViewModel.LoadProject();
				projectEditViewModel.State = Mvvm.ViewModelState.Loaded;
			}
		}

		public Command CreateProject { get; private set; }
		private async void OnCreateProjectExecute()
		{
			actGeneratorViewModel.CloseOpenedDialog.Execute();
			await DialogHost.Show(createProjectDialog, DialogHostId);
			if(createProjectDialog.DataContext is CreateProjectDialogViewModel createProjectDialogViewModel && createProjectDialogViewModel.IsCreate)
			{
				projectEditViewModel.State = Mvvm.ViewModelState.Initializing;
				SelectedTabIndex = 1;
				projectEditViewModel.CreationProjectName = createProjectDialogViewModel.ProjectName;
				await projectEditViewModel.CreateProject();
				await projectEditViewModel.LoadProject();
				projectEditViewModel.State = Mvvm.ViewModelState.Loaded;
			}
		}

		public Command<ProjectEditViewModel> BindProjectEditor { get; private set; }
		private void OnBindProjectEditorExecute(ProjectEditViewModel projectEdit)
		{
			projectEditViewModel = projectEdit;
		}

		public Command RestoreProject { get; private set; }
		private void OnRestoreProjectExecute()
		{
			SelectedTabIndex = 2;
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

		#endregion
	}
}
