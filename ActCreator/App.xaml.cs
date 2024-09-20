using ActCreator.Settings;
using SendException;
using System;
using System.Windows;

namespace ActCreator
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public App()
		{
			SendExceptionProcessor.Init(ProgramSettings.DirectoryPath, -1002032139384);
			Current.DispatcherUnhandledException += (s, e) =>
			{
				SendExceptionWindow.ShowDialog(e.Exception);
				e.Handled = true;
			};
			AppDomain.CurrentDomain.UnhandledException += (s, e) =>
			{
				SendExceptionWindow.ShowDialog(e.ExceptionObject as Exception);
			};
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			MainWindow window = new MainWindow(e.Args);
			window.Show();
		}
	}
}
