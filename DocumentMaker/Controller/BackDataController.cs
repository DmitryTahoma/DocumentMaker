using DocumentMaker.Model;

namespace DocumentMaker.Controller
{
    public class BackDataController
    {
        private BackDataModel model;

        public BackDataController()
        {
            model = new BackDataModel();
        }

        public BackDataController(BackDataModel _model)
        {
            if(_model != null)
            {
                model = _model;
            }
            else
            {
                model = new BackDataModel();
            }
        }

        public uint Id { get => model.Id; set => model.Id = value; }
        public BackType Type { get => model.Type; set => model.Type = value; }
        public string BackNumberText { get => model.BackNumberText; set => model.BackNumberText = value; }
        public string BackName { get => model.BackName; set => model.BackName = value; }
        public string BackCountRegionsText { get => model.BackCountRegionsText; set => model.BackCountRegionsText = value; }
        public bool IsRework { get => model.IsRework; set => model.IsRework = value; }
        public string SpentTimeText { get => model.SpentTimeText; set => model.SpentTimeText = value; }

        public BackDataModel GetModel()
        {
            return model;
        }
    }
}
