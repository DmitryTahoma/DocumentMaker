using Dml;
using Dml.Controller;
using DocumentMaker.Model.Back;
using DocumentMaker.Model.Controls;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace DocumentMaker.Controller.Controls
{
	public class FullBackDataController : BaseBackDataController
	{
		private readonly FullBackDataModel model;

		public FullBackDataController() : base(new FullBackDataModel())
		{
			model = base.GetModel() as FullBackDataModel;
			Load();
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
			Load();
		}

		public string WeightText { get => model.WeightText; set => model.WeightText = value; }
		public string SumText { get => model.SumText; set => model.SumText = value; }
		public uint WorkObjectId { get => model.WorkObjectId; set => model.WorkObjectId = value; }
		public uint ActSum { get; set; }
		public ObservableRangeCollection<WorkObject> WorkTypesList => model.WorkTypesList;

		public new FullBackDataModel GetModel()
		{
			return model;
		}
	}
}
