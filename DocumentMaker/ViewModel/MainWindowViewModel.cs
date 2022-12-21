using System.Windows;

namespace DocumentMaker.ViewModel
{
	public class MainWindowViewModel : DependencyObject
	{
		#region Properties

		public string[] ApplicationArgs { get; set; } = null;

		#endregion
	}
}
