using System.Windows;
using System.Windows.Input;

namespace DocumentMaker.View
{
	/// <summary>
	/// Interaction logic for CorrectSupportWindow.xaml
	/// </summary>
	public partial class CorrectSupportWindow : Window
	{
		public CorrectSupportWindow()
		{
			InitializeComponent();
			DataContext = this;
		}

		public bool IsCorrection { get; private set; }
		public string NumberText { get; set; }
		public bool TakeSumFromDevelopment { get; set; }

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
