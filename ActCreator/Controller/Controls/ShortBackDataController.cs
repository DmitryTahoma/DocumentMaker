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
			Load();
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
			Load();
		}

		public new ShortBackDataModel GetModel()
		{
			return model;
		}
	}
}
