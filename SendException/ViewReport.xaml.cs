using System.Windows;
using Telegram.Bot.Types;

namespace SendException
{
	/// <summary>
	/// Логика взаимодействия для ViewReport.xaml
	/// </summary>
	public partial class ViewReport : Window
	{
		public ViewReport(string _sReport)
		{
			InitializeComponent();
			TextBlockDecriptionExc.Text = _sReport;
		}

		private void ControlKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == System.Windows.Input.Key.Escape)
				Close();
		}
	}
}
