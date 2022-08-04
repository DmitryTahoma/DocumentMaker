using Dml.Controller.Validation;
using Dml.Model;
using Dml.Model.Back;
using System.Collections.Generic;

namespace Dml.Controller
{
    public class BackDataController
    {
        private readonly StringValidator validator;
        private BackDataModel model;

        public BackDataController()
        {
            validator = new StringValidator();
            model = new BackDataModel();
        }

        public BackDataController(BackDataModel _model)
        {
            validator = new StringValidator();
            if (_model != null)
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
        public string GameName { get => model.GameName; set => model.GameName = value; }
        public bool IsRework { get => model.IsRework; set => model.IsRework = value; }
        public bool IsSketch { get => model.IsSketch; set => model.IsSketch = value; }
        public string SpentTimeText { get => model.SpentTimeText; set => model.SpentTimeText = value; }
        public string OtherText { get => model.OtherText; set => model.OtherText = value; }
        public IList<BackDataType> BackDataTypesList => model.BackDataTypesList;

        public BackDataModel GetModel()
        {
            return model;
        }

        public bool Validate(ref string errorText)
        {
            errorText = "Строка таблиці №" + Id.ToString() + ": ";

            if (Type == BackType.Other && !validator.IsFree(OtherText))
                errorText += "Строка з текстом не може бути пустою.";
            else if (Type != BackType.Other)
            {
                if (Type != BackType.Craft && !validator.IsFree(BackNumberText))
                    errorText += "Строка \"Номер беку\" не може бути пустою.";
                else if (!validator.IsFree(BackName))
                    errorText += "Строка \"Ім’я беку\" не може бути пустою.";
                else if ((Type == BackType.Regions || Type == BackType.HogRegions) && !validator.IsDigit(BackCountRegionsText))
                    errorText += "Кількість регіонів невірно введена.\nПриклад: 11";
                else if (!validator.IsFree(GameName))
                    errorText += "Строка \"Назва гри\" не може бути пустою.";
                else if (!validator.IsDigit(SpentTimeText))
                    errorText += "Затрачений час невірно введений.\nПриклад: 7";
                else
                    return true;
            }
            else
                return true;

            return false;
        }
    }
}
