using ActGenerator.Model;
using Dml.Controller.Validation;
using Mvvm.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ActGenerator.ViewModel
{
	class MainWindowViewModel : DependencyObject
	{
		ActGeneratorSession model = new ActGeneratorSession();
		ActGeneratorViewModel actGeneratorViewModel = null;

		public MainWindowViewModel()
		{
			InitCommands();
		}

		#region Properties

		public int SelectedTabIndex
		{
			get { return (int)GetValue(SelectedTabIndexProperty); }
			set { SetValue(SelectedTabIndexProperty, value); }
		}
		public static readonly DependencyProperty SelectedTabIndexProperty = DependencyProperty.Register(nameof(SelectedTabIndex), typeof(int), typeof(MainWindowViewModel));

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
		private void OnOpenProjectExecute()
		{
			SelectedTabIndex = 1;
		}

		public Command CreateProject { get; private set; }
		private void OnCreateProjectExecute()
		{
			SelectedTabIndex = 1;
		}

		#endregion
	}
}
