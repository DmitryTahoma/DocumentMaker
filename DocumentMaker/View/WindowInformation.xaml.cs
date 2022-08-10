using Dml.Controls;
using Dml.Model.Files;
using DocumentMaker.Controller;
using DocumentMaker.Model.OfficeFiles.Human;
using System.Collections.Generic;
using System.Windows;

namespace DocumentMaker.View
{
	/// <summary>
	/// Логика взаимодействия для WindowInformation.xaml
	/// </summary>
	public partial class WindowInformation : Window
	{
		private readonly WindowInformationController controller;

		public IList<HumanData> HumanFullNameList => controller.HumanFullNameList;

		public static readonly DependencyProperty SelectedHumanProperty;
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
			SelectedHumanProperty = DependencyProperty.Register("FullHumanName", typeof(string), typeof(WindowInformation));
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

		public string SelectedHuman
		{
			get => (string)GetValue(SelectedHumanProperty);
			set
			{
				SetValue(SelectedHumanProperty, value);
				controller.SelectedHuman = value;
			}
		}

		public string HumanIdText
		{
			get => (string)GetValue(HumanIdTextProperty);
			set
			{
				SetValue(HumanIdTextProperty, value);
				controller.HumanIdText = value;
			}
		}

		public string AddressText
		{
			get => (string)GetValue(AddressTextProperty);
			set
			{
				SetValue(AddressTextProperty, value);
				controller.AddressText = value;
			}
		}

		public string PaymentAccountText
		{
			get => (string)GetValue(PaymentAccountTextProperty);
			set
			{
				SetValue(PaymentAccountTextProperty, value);
				controller.PaymentAccountText = value;
			}
		}

		public string BankName
		{
			get => (string)GetValue(BankNameProperty);
			set
			{
				SetValue(BankNameProperty, value);
				controller.BankName = value;
			}
		}

		public string MfoText
		{
			get => (string)GetValue(MfoTextProperty);
			set
			{
				SetValue(MfoTextProperty, value);
				controller.MfoText = value;
			}
		}

		public string ContractNumberText
		{
			get => (string)GetValue(ContractNumberTextProperty);
			set
			{
				SetValue(ContractNumberTextProperty, value);
				controller.ContractNumberText = value;
			}
		}

		public string ContractDateText
		{
			get => (string)GetValue(ContractDateTextProperty);
			set
			{
				SetValue(ContractDateTextProperty, value);
				controller.ContractDateText = value;
			}
		}

		public string ContractReworkNumberText
		{
			get => (string)GetValue(ContractReworkNumberTextProperty);
			set
			{
				SetValue(ContractReworkNumberTextProperty, value);
				controller.ContractReworkNumberText = value;
			}
		}

		public string ContractReworkDateText
		{
			get => (string)GetValue(ContractReworkDateTextProperty);
			set
			{
				SetValue(ContractReworkDateTextProperty, value);
				controller.ContractReworkDateText = value;
			}
		}

		public WindowInformation()
		{
			controller = new WindowInformationController();
			controller.Load();

			InitializeComponent();
		}

		public WindowInformation(DmxFile file)
		{
			controller = new WindowInformationController();
			controller.Load();

			InitializeComponent();

			SelectedHuman = file.SelectedHuman;
		}

		private void WindowLoaded(object sender, RoutedEventArgs e)
		{
			if (controller != null)
			{
				SetDataFromController();
			}
		}

		private void ChangedHuman(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			if (controller != null
				&& sender is System.Windows.Controls.ComboBox comboBox
				&& comboBox.SelectedItem is HumanData humanData)
			{
				controller.SetHuman(humanData);
				SetDataFromController();
			}
		}

		private void SetDataFromController()
		{
			HumanFullNameComboBox.Text = controller.SelectedHuman;
			HumanIdTextInput.InputText = controller.HumanIdText;
			AddressTextInput.InputText = controller.AddressText;
			PaymentAccountTextInput.InputText = controller.PaymentAccountText;
			BankNameInput.InputText = controller.BankName;
			MfoTextInput.InputText = controller.MfoText;
			ContractNumberTextInput.InputText = controller.ContractNumberText;
			ContractDateTextInput.InputText = controller.ContractDateText;
			ContractReworkNumberTextInput.InputText = controller.ContractReworkNumberText;
			ContractReworkDateTextInput.InputText = controller.ContractReworkDateText;
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
