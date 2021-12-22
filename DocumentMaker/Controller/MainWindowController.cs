using System.Collections.Generic;

namespace DocumentMaker.Controller
{
    public class MainWindowController
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
        public List<BackDataController> BackDataControllers { get; set; }
    }
}
