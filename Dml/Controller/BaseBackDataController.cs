using Dml.Controller.Validation;
using Dml.Model;
using Dml.Model.Back;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Dml.Controller
{
	public abstract class BaseBackDataController
	{
		private const string gameNamesFile = "projectnames.xml";

		protected readonly StringValidator validator;
		private readonly BaseBackDataModel model;

		public BaseBackDataController(BaseBackDataModel _model)
		{
			validator = new StringValidator();
			model = _model;
			Load();
		}

		public uint Id { get => model.Id; set => model.Id = value; }
		public BackType Type { get => model.Type; set => model.Type = value; }
		public string BackNumberText { get => model.BackNumberText; set => model.BackNumberText = value; }
		public string BackName { get => model.BackName; set => model.BackName = value; }
		public string BackCountRegionsText { get => model.BackCountRegionsText; set => model.BackCountRegionsText = value; }
		public IList<string> GameNameList => model.GameNameList;
		public string GameName { get => model.GameName; set => model.GameName = value; }
		public bool IsRework { get => model.IsRework; set => model.IsRework = value; }
		public bool IsSketch { get => model.IsSketch; set => model.IsSketch = value; }
		public string SpentTimeText { get => model.SpentTimeText; set => model.SpentTimeText = value; }
		public string OtherText { get => model.OtherText; set => model.OtherText = value; }
		public IList<BackDataType> BackDataTypesList => model.BackDataTypesList;

		public virtual BaseBackDataModel GetModel()
		{
			return model;
		}

		public virtual bool Validate(ref string errorText)
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
				else if ((Type == BackType.Regions || Type == BackType.HogRegions) && !validator.IsFree(BackCountRegionsText))
					errorText += "Кількість регіонів не може бути пустою.\nПриклад: 11";
				else if ((Type == BackType.Regions || Type == BackType.HogRegions) && validator.IsEndMinus(BackCountRegionsText))
					errorText += "Регіони записані некорректно. Незавершена послідовність.\nПриклад: 1-2, 3-4, 8-15";
				else if ((Type == BackType.Regions || Type == BackType.HogRegions) && !validator.IsOrderRegions(BackCountRegionsText))
					errorText += "Регіони записані некорректно. Кінець послідовності менший за початок.\nПриклад: 1-2, 3-4, 8-15";
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

		public virtual void Load()
		{
			string gameNamesFullpath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), gameNamesFile);
			model.LoadGameNames(gameNamesFullpath);
		}
	}
}
