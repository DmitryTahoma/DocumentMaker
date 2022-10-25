using ActGenerator.Model;
using Dml.Controller.Validation;
using Mvvm.Commands;
using System.Windows;

namespace ActGenerator.ViewModel
{
	class MainWindowViewModel
	{
		ActGeneratorSession model = new ActGeneratorSession();

		public MainWindowViewModel()
		{
			InitCommands();
		}

		#region Commands

		private void InitCommands()
		{
			LoadSession = new Command<MainWindow>(OnLoadSessionExecute);
			SaveSession = new Command<MainWindow>(OnSaveSessionExecute);
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
		}

		public Command<MainWindow> SaveSession { get; private set; }
		private void OnSaveSessionExecute(MainWindow window)
		{
			model.WindowTop = window.Top;
			model.WindowLeft = window.Left;
			model.WindowHeight = window.Height;
			model.WindowWidth = window.Width;
			model.WindowState = window.WindowState == WindowState.Minimized ? WindowState.Normal : window.WindowState;

			model.Save();
		}

		#endregion
	}
}
