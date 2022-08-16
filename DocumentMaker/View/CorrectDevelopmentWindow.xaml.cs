using System.Windows;
using System.Windows.Input;

namespace DocumentMaker.View
{
	/// <summary>
	/// Interaction logic for CorrectDevelopmentWindow.xaml
	/// </summary>
	public partial class CorrectDevelopmentWindow : Window
	{
		public CorrectDevelopmentWindow()
		{
			InitializeComponent();
			DataContext = this;
		}

		public bool IsCorrection { get; private set; }
		public string NumberText { get; set; }
		public bool TakeSumFromSupport { get; set; }

		private void WindowKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
			{
				Close();
			}
		}

		private void CorrectionClick(object sender, RoutedEventArgs e)
		{
			IsCorrection = true;
			Close();
		}
	}
}
