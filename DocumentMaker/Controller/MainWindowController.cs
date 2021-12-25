using DocumentMaker.Model;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace DocumentMaker.Controller
{
    public class MainWindowController
    {
        private const string saveFile = "session.xml";

        private DocumentMakerModel model;

        public MainWindowController()
        {
            model = new DocumentMakerModel();
            BackDataControllers = new List<BackDataController>();
        }

        public string TechnicalTaskDateText { get => model.TechnicalTaskDateText; set => model.TechnicalTaskDateText = value; }
        public string ActDateText { get => model.ActDateText; set => model.ActDateText = value; }
        public string AdditionNumText { get => model.AdditionNumText; set => model.AdditionNumText = value; }
        public string FullHumanName { get => model.FullHumanName; set => model.FullHumanName = value; }
        public string HumanIdText { get => model.HumanIdText; set => model.HumanIdText = value; }
        public string AddressText { get => model.AddressText; set => model.AddressText = value; }
        public string PaymentAccountText { get => model.PaymentAccountText; set => model.PaymentAccountText = value; }
        public string BankName { get => model.BankName; set => model.BankName = value; }
        public string MfoText { get => model.MfoText; set => model.MfoText = value; }
        public string ContractNumberText { get => model.ContractNumberText; set => model.ContractNumberText = value; }
        public string ContractDateText { get => model.ContractDateText; set => model.ContractDateText = value; }
        public List<BackDataController> BackDataControllers { get; set; }

        public void Save()
        {
            string fullpath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), saveFile);

            List<BackDataModel> backDataModels = new List<BackDataModel>();
            foreach (BackDataController controller in BackDataControllers)
            {
                backDataModels.Add(controller.GetModel());
            }

            model.Save(fullpath, backDataModels);
        }

        public void Load()
        {
            string fullpath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), saveFile);

            model.Load(fullpath, out List<BackDataModel> backModels);
            foreach (BackDataModel model in backModels)
            {
                BackDataControllers.Add(new BackDataController(model));
            }
        }

        public void Export(string path)
        {
            List<BackDataModel> backDataModels = new List<BackDataModel>();
            foreach (BackDataController controller in BackDataControllers)
            {
                backDataModels.Add(controller.GetModel());
            }

            model.Export(path, backDataModels);
        }
    }
}
