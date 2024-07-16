using ActCreator.Model;
using Dml.Controller;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ActCreator.Controller.Controls
{
	public class ShortBackDataController : BaseBackDataController
	{
		private readonly ShortBackDataModel model;

		public ShortBackDataController() : base(new ShortBackDataModel())
		{
			model = base.GetModel() as ShortBackDataModel;
		}

		public ShortBackDataController(ShortBackDataModel _model) : base(_model)
		{
			if (_model != null)
			{
				model = _model;
			}
			else
			{
				model = new ShortBackDataModel();
			}
		}

		public new ShortBackDataModel GetModel()
		{
			return model;
		}

		public override bool Validate(ref string errorText)
		{
			if (base.Validate(ref errorText))
			{
				if (!validator.IsDigit(SpentTimeText))
					errorText += "Затрачений час невірно введений.\nПриклад: 7";
				else if(Type == Dml.Model.Back.BackType.Other)
				{
					int countKavichka = OtherText.Count(c => c == '"');
					if (countKavichka % 2 == 1)
					{
						errorText += "Не закриваються лапки.";
					}
					else
						return true;
				}
				else
					return true;
			}
			return false;
		}

		public bool HaveWarnings(ref string warningText)
		{
			if (Type == Dml.Model.Back.BackType.Other)
			{
				List<int> indexes = new List<int>();
				int i = 0;
				foreach (char c in OtherText)
				{
					if (c == '"')
					{
						indexes.Add(i);
					}
					++i;
				}
				if (indexes.Count == 0)
				{
					if (!string.IsNullOrEmpty(warningText))
					{
						warningText += "\n";
					}
					warningText += "Строка таблиці №" + Id.ToString() + ": Не вказана назва гри.";
					return true;
				}
				else
				{
					bool haveGameName = false;
					for (i = 0; i < indexes.Count; i += 2)
					{
						int indexStart = indexes[i] + 1;
						int length = indexes[i + 1] - indexStart;
						string checkGameName = OtherText.Substring(indexStart, length);
						if (GameNameList.Where(gameObject => gameObject.Name == checkGameName).Count() > 0)
						{
							haveGameName = true;
							break;
						}
					}
					
					if (!haveGameName)
					{
						if (!string.IsNullOrEmpty(warningText))
						{
							warningText += "\n";
						}
						warningText += "Строка таблиці №" + Id.ToString() + ": Не існує такої гри.";
						return true;
					}
				}
			}
			return false;
		}
	}
}
