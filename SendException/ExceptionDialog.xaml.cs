using MaterialDesignThemes.Wpf;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SendException
{
	/// <summary>
	/// Логика взаимодействия для ExceptionDialog.xaml
	/// </summary>
	public partial class ExceptionDialog : Window
    {
        private StringBuilder Report;
        private Exception excException = null;
        private bool bSendReport = false;
        private long chatId = 0;
        private Version version;

		public static readonly DependencyProperty DecriptionExcProperty;

		static ExceptionDialog()
		{
			DecriptionExcProperty = DependencyProperty.Register("DecriptionExc", typeof(string), typeof(ExceptionDialog));
		}

		public ExceptionDialog(Exception exc, long _chatId, Version _version)
		{
			InitializeComponent();
			excException = exc;
            chatId = _chatId;
            version = _version;
        }

		public string DecriptionExc
		{
			get => (string)GetValue(DecriptionExcProperty);
			set => SetValue(DecriptionExcProperty, value);
		}

		private void DialogLoaded(object sender, RoutedEventArgs e)
		{
			Report = new StringBuilder();

			string sException = "";
			if (excException != null)
				sException = excException.ToString();

			Report.AppendFormat("Version: {0}", version);
			Report.AppendFormat("\nPC: {0}", Environment.MachineName);
			Report.AppendFormat("\nUser: {0}", Environment.UserName);
			Report.AppendFormat("\nDateTime: {0}\n\n", DateTime.Now.ToString());
			Report.AppendLine("---------- Exception Info------------");
			Report.AppendLine(sException);
		}

		private void DialogClosed(object sender, RoutedEventArgs e)
		{
			SendReport(true);
		}

		private void SendReportExc(object sender, RoutedEventArgs e)
		{
			SendReport(false);
			Close();
		}

		private async void SendReport(bool _bFormClosed)
		{
			try
			{
				if (!bSendReport)
				{
					btSendReport.IsEnabled = false;
					bSendReport = true;
					Report.AppendLine("-----------------User Info----------------");
					Report.AppendLine(DecriptionExc);
					TelegramBotClient client = new TelegramBotClient("6454432469:AAHvawu4O6zfS1AxtmUDsfnfNEBAa5HqFmE");
					string str = Report.ToString();
					const int maxLength = 4096;
					for (int i = 0; i < Report.Length / maxLength + 1; ++i)
					{
						int stIndex = i * maxLength;
						int endIndex = stIndex + maxLength;
						string msg;
						if (endIndex > str.Length)
							msg = str.Substring(stIndex);
						else
							msg = str.Substring(stIndex, maxLength);

						await client.SendTextMessageAsync(new ChatId(chatId), msg);
					}
				}
			}
			catch (Exception exc)
			{
				if (!_bFormClosed)
					MessageBox.Show(exc.ToString(), "Send Report Exception");
			}
		}

		private void ControlKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == System.Windows.Input.Key.Escape)
				Close();
		}
	}
}
