using Dml.Controller;
using DocumentMaker.Model.Controls;

namespace DocumentMaker.Controller.Controls
{
	public class FullBackDataController : BaseBackDataController
	{
		private readonly FullBackDataModel model;

		public FullBackDataController() : base(new FullBackDataModel())
		{
			model = base.GetModel() as FullBackDataModel;
		}

		public FullBackDataController(FullBackDataModel _model) : base(_model)
		{
			if (_model != null)
			{
				model = _model;
			}
			else
			{
				model = new FullBackDataModel();
			}
		}

		public new FullBackDataModel GetModel()
		{
			return model;
		}
	}
}
