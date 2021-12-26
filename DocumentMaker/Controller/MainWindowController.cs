using DocumentMaker.Controller.Validation;
using DocumentMaker.Model;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace DocumentMaker.Controller
{
    public class MainWindowController
    {
        private const string saveFile = "session.xml";

        private readonly StringValidator validator;
        private DocumentMakerModel model;

        public MainWindowController()
        {
            validator = new StringValidator();
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
        public bool HasNoMovedFiles => model.HasNoMovedFiles;

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

        public bool Validate(out string errorText)
        {
            errorText = "";
            bool isValidGeneralData = false;

            if (!validator.IsDate(TechnicalTaskDateText))
                errorText = "Невірно заповнена дата тех.завдання.\nПриклад: 20.07.2021";
            else if (!validator.IsDate(ActDateText))
                errorText = "Невірно заповнена дата акту.\nПриклад: 20.07.2021";
            else if (!validator.IsDigit(AdditionNumText))
                errorText = "Невірно заповнений номер додатку.\nПриклад: 1";
            else if (!validator.IsFullName(FullHumanName))
                errorText = "Невірно заповнена строка з повним ім’ям.\nПриклад: Іванов Іван Іванович";
            else if (!validator.IsFree(HumanIdText))
                errorText = "Строка \"ІН\" не може бути пустою.";
            else if (!validator.IsFree(AddressText))
                errorText = "Строка \"Адреса проживання\" не може бути пустою.";
            else if (!validator.IsFree(PaymentAccountText))
                errorText = "Строка \"р/р\" не може бути пустою.";
            else if (!validator.IsFree(BankName))
                errorText = "Строка \"Банк\" не може бути пустою.";
            else if (!validator.IsFree(MfoText))
                errorText = "Строка \"МФО\" не може бути пустою.";
            else if (!validator.IsFree(ContractNumberText))
                errorText = "Строка \"Номер договору\" не може бути пустою.";
            else if (!validator.IsFree(ContractDateText))
                errorText = "Строка \"Дата складання договору\" не може бути пустою.";
            else
                isValidGeneralData = true;

            if (isValidGeneralData)
            {
                foreach (BackDataController backDataController in BackDataControllers)
                {
                    if (!backDataController.Validate(ref errorText))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public string GetInfoNoMovedFiles()
        {
            IEnumerable<KeyValuePair<string, string>> files = model.GetInfoNoMovedFiles();
            string res = "";

            foreach (KeyValuePair<string, string> file in files)
            {
                res += file.Value + "\n";
            }

            return res;
        }

        public void ReplaceCreatedFiles()
        {
            model.ReplaceCreatedFiles();
        }

        public void RemoveTemplates()
        {
            model.RemoveTemplates();
        }
    }
}
