using DocumentMaker.Model.OfficeFiles;
using DocumentMaker.Model.Session;
using System.Collections.Generic;

namespace DocumentMaker.Model
{
    public class DocumentMakerModel
    {
        public string TechnicalTaskDateText { get; set; }
        public string ActDateText { get; set; }
        public string AdditionNumText { get; set; }
        public string FullHumanName { get; set; }
        public string HumanIdText { get; set; }
        public string AddressText { get; set; }
        public string PaymentAccountText { get; set; }
        public string BankName { get; set; }
        public string MfoText { get; set; }
        public string ContractNumberText { get; set; }
        public string ContractDateText { get; set; }

        public void Save(string path, IEnumerable<BackDataModel> backModels)
        {
            XmlSaver saver = new XmlSaver();
            saver.AppendAllProperties(this);

            foreach (BackDataModel backDataModel in backModels)
            {
                saver.CreateBackNode();
                saver.AppendAllBackProperties(backDataModel);
                saver.PushBackNode();
            }

            saver.Save(path);
        }

        public void Load(string path, out List<BackDataModel> backModels)
        {
            backModels = new List<BackDataModel>();

            XmlLoader loader = new XmlLoader();
            if (loader.TryLoad(path))
            {
                loader.SetLoadedProperties(this);
                loader.SetLoadedBacksProperties(backModels);
            }
        }

        public void Export(string path, IEnumerable<BackDataModel> backModels)
        {
            OfficeExporter exporter = new OfficeExporter();
            exporter.ExportWordTemplate("DocumentMakerTemplate01.docx");
            exporter.FillGeneralData(new DocumentGeneralData(this));
            exporter.FillTableData(new DocumentTableData(backModels));
            exporter.SaveTemplate(path, "DocumentMakerTemplate01.docx");
        }
    }
}
