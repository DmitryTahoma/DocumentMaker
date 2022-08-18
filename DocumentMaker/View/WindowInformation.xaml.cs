using DocumentMaker.Model.OfficeFiles.Human;
using System.Windows;

namespace DocumentMaker.View
{
	/// <summary>
	/// Логика взаимодействия для WindowInformation.xaml
	/// </summary>
	public partial class WindowInformation : Window
	{
		public static readonly DependencyProperty HumanNameProperty;
		public static readonly DependencyProperty HumanIdTextProperty;
		public static readonly DependencyProperty AddressTextProperty;
		public static readonly DependencyProperty PaymentAccountTextProperty;
		public static readonly DependencyProperty BankNameProperty;
		public static readonly DependencyProperty MfoTextProperty;
		public static readonly DependencyProperty ContractNumberTextProperty;
		public static readonly DependencyProperty ContractDateTextProperty;
		public static readonly DependencyProperty ContractReworkNumberTextProperty;
		public static readonly DependencyProperty ContractReworkDateTextProperty;

		static WindowInformation()
		{
			HumanNameProperty = DependencyProperty.Register("HumanName", typeof(string), typeof(WindowInformation));
			HumanIdTextProperty = DependencyProperty.Register("HumanIdText", typeof(string), typeof(WindowInformation));
			AddressTextProperty = DependencyProperty.Register("AddressText", typeof(string), typeof(WindowInformation));
			PaymentAccountTextProperty = DependencyProperty.Register("PaymentAccountText", typeof(string), typeof(WindowInformation));
			BankNameProperty = DependencyProperty.Register("BankName", typeof(string), typeof(WindowInformation));
			MfoTextProperty = DependencyProperty.Register("MfoText", typeof(string), typeof(WindowInformation));
			ContractNumberTextProperty = DependencyProperty.Register("ContractNumberText", typeof(string), typeof(WindowInformation));
			ContractDateTextProperty = DependencyProperty.Register("ContractDateText", typeof(string), typeof(WindowInformation));
			ContractReworkNumberTextProperty = DependencyProperty.Register("ContractReworkNumberText", typeof(string), typeof(WindowInformation));
			ContractReworkDateTextProperty = DependencyProperty.Register("ContractReworkDateText", typeof(string), typeof(WindowInformation));
		}

		public WindowInformation()
		{
			InitializeComponent();
		}

		public WindowInformation(HumanData humanData) : this()
		{
			if (humanData != null)
			{
				HumanName = humanData.Name;
				HumanIdText = humanData.HumanIdText;
				BankName = humanData.BankName;
				PaymentAccountText = humanData.PaymentAccountText;
				ContractNumberText = humanData.ContractNumberText;
				ContractDateText = humanData.ContractDateText;
				ContractReworkNumberText = humanData.ContractReworkNumberText;
				ContractReworkDateText = humanData.ContractReworkDateText;
				AddressText = humanData.CityName + ' ' + humanData.AddressText;
				MfoText = humanData.MfoText;
			}
			else
			{
				HumanName = HumanIdText = BankName = PaymentAccountText = ContractNumberText = ContractDateText = ContractReworkNumberText = ContractReworkDateText = AddressText = MfoText = "<error-human>";
			}
		}

		public string HumanName
		{
			get => (string)GetValue(HumanNameProperty);
			set => SetValue(HumanNameProperty, value);
		}

		public string HumanIdText
		{
			get => (string)GetValue(HumanIdTextProperty);
			set => SetValue(HumanIdTextProperty, value);
		}

		public string AddressText
		{
			get => (string)GetValue(AddressTextProperty);
			set => SetValue(AddressTextProperty, value);
		}

		public string PaymentAccountText
		{
			get => (string)GetValue(PaymentAccountTextProperty);
			set => SetValue(PaymentAccountTextProperty, value);
		}

		public string BankName
		{
			get => (string)GetValue(BankNameProperty);
			set => SetValue(BankNameProperty, value);
		}

		public string MfoText
		{
			get => (string)GetValue(MfoTextProperty);
			set => SetValue(MfoTextProperty, value);
		}

		public string ContractNumberText
		{
			get => (string)GetValue(ContractNumberTextProperty);
			set => SetValue(ContractNumberTextProperty, value);
		}

		public string ContractDateText
		{
			get => (string)GetValue(ContractDateTextProperty);
			set => SetValue(ContractDateTextProperty, value);
		}

		public string ContractReworkNumberText
		{
			get => (string)GetValue(ContractReworkNumberTextProperty);
			set => SetValue(ContractReworkNumberTextProperty, value);
		}

		public string ContractReworkDateText
		{
			get => (string)GetValue(ContractReworkDateTextProperty);
			set => SetValue(ContractReworkDateTextProperty, value);
		}

		private void WindowKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == System.Windows.Input.Key.Escape)
			{
				Close();
			}
		}
	}
}
