using DocumentMakerModelLibrary.OfficeFiles.Human;
using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Controls;

namespace DocumentMaker.View.Dialogs
{
	/// <summary>
	/// Interaction logic for HumanInformationDialog.xaml
	/// </summary>
	public partial class HumanInformationDialog : UserControl
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

		static HumanInformationDialog()
		{
			HumanNameProperty = DependencyProperty.Register("HumanName", typeof(string), typeof(HumanInformationDialog));
			HumanIdTextProperty = DependencyProperty.Register("HumanIdText", typeof(string), typeof(HumanInformationDialog));
			AddressTextProperty = DependencyProperty.Register("AddressText", typeof(string), typeof(HumanInformationDialog));
			PaymentAccountTextProperty = DependencyProperty.Register("PaymentAccountText", typeof(string), typeof(HumanInformationDialog));
			BankNameProperty = DependencyProperty.Register("BankName", typeof(string), typeof(HumanInformationDialog));
			MfoTextProperty = DependencyProperty.Register("MfoText", typeof(string), typeof(HumanInformationDialog));
			ContractNumberTextProperty = DependencyProperty.Register("ContractNumberText", typeof(string), typeof(HumanInformationDialog));
			ContractDateTextProperty = DependencyProperty.Register("ContractDateText", typeof(string), typeof(HumanInformationDialog));
			ContractReworkNumberTextProperty = DependencyProperty.Register("ContractReworkNumberText", typeof(string), typeof(HumanInformationDialog));
			ContractReworkDateTextProperty = DependencyProperty.Register("ContractReworkDateText", typeof(string), typeof(HumanInformationDialog));
		}

		public HumanInformationDialog()
		{
			InitializeComponent();
		}

		public HumanInformationDialog(HumanData humanData) : this()
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

		private void ControlKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == System.Windows.Input.Key.Escape)
			{
				DialogHost.CloseDialogCommand.Execute(null, null);
			}
		}
	}
}
