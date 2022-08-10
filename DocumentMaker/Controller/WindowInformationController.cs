using Dml.Controller;
using Dml.Controller.Validation;
using Dml.Model;
using Dml.Model.Files;
using Dml.Model.Template;
using DocumentMaker.Model;
using DocumentMaker.Model.OfficeFiles.Human;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;

namespace DocumentMaker.Controller
{
	public class WindowInformationController
	{
		private const string saveFile = "session.xml";
		private const string humansFile = "HumanData.xlsx";

		private readonly DocumentMakerModel model;

		public WindowInformationController()
		{
			model = new DocumentMakerModel();
		}

		#region Window settings

		//public double WindowTop { get => model.WindowTop; set => model.WindowTop = value; }
		//public double WindowLeft { get => model.WindowLeft; set => model.WindowLeft = value; }
		//public double WindowHeight { get => model.WindowHeight; set => model.WindowHeight = value; }
		//public double WindowWidth { get => model.WindowWidth; set => model.WindowWidth = value; }
		//public WindowState WindowState { get => model.WindowState; set => model.WindowState = value; }

		#endregion

		public string SelectedHuman { get => model.SelectedHuman; set => model.SelectedHuman = value; }
		public string HumanIdText { get => model.HumanIdText; set => model.HumanIdText = value; }
		public string AddressText { get => model.AddressText; set => model.AddressText = value; }
		public string PaymentAccountText { get => model.PaymentAccountText; set => model.PaymentAccountText = value; }
		public string BankName { get => model.BankName; set => model.BankName = value; }
		public string MfoText { get => model.MfoText; set => model.MfoText = value; }
		public string ContractNumberText { get => model.ContractNumberText; set => model.ContractNumberText = value; }
		public string ContractDateText { get => model.ContractDateText; set => model.ContractDateText = value; }
		public string ContractReworkNumberText { get => model.ContractReworkNumberText; set => model.ContractReworkNumberText = value; }
		public string ContractReworkDateText { get => model.ContractReworkDateText; set => model.ContractReworkDateText = value; }
		public IList<HumanData> HumanFullNameList => model.HumanFullNameList;

		public void Load()
		{
			string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			string humansFullpath = Path.Combine(path, humansFile);
			model.LoadHumans(humansFullpath);
		}

		public void SetHuman(HumanData humanData)
		{
			SelectedHuman = humanData.Name;
			HumanIdText = humanData.HumanIdText;
			BankName = humanData.BankName;
			PaymentAccountText = humanData.PaymentAccountText;
			ContractNumberText = humanData.ContractNumberText;
			ContractDateText = humanData.ContractDateText;
			ContractReworkNumberText = humanData.ContractReworkNumberText;
			ContractReworkDateText = humanData.ContractReworkDateText;
			AddressText = humanData.AddressText;
			MfoText = humanData.MfoText;
		}
	}
}
