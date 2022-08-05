using System.Windows;

namespace DocumentMaker
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			MainWindow window = new MainWindow(e.Args);
			window.Show();
		}
	}
}
