using ActCreator.Model;
using Dml.Controller;

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
				else
					return true;
			}
			return false;
		}
	}
}
