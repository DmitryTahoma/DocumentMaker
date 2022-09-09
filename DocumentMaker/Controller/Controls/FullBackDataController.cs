using Dml;
using Dml.Controller;
using Dml.Model.Template;
using Dml.UndoRedo;
using DocumentMaker.Model.Back;
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

		public string WeightText { get => model.WeightText; set => model.WeightText = value; }
		public double Weight { get => model.Weight; set => model.Weight = value; }
		public string SumText { get => model.SumText; set => model.SumText = value; }
		public uint WorkObjectId { get => model.WorkObjectId; set => model.WorkObjectId = value; }
		public uint ActSum { get; set; }
		public bool IsOtherType { get => model.IsOtherType; set => model.IsOtherType = value; }
		public ObservableRangeCollection<WorkObject> WorkTypesList => model.WorkTypesList;

		public new FullBackDataModel GetModel()
		{
			return model;
		}

		public override bool Validate(ref string errorText)
		{
			if (base.Validate(ref errorText))
			{
				if (IsOtherType) errorText.Insert(0, "[Інше] ");
				else if (IsRework) errorText.Insert(0, "[Підтримка] ");
				else errorText.Insert(0, "[Розробка] ");

				if (!uint.TryParse(SumText, out uint sumText) || sumText == 0)
					errorText += "Сума не може бути нульовою.";
				else
					return true;
			}
			return false;
		}

		public override void TrimAllStrings()
		{
			base.TrimAllStrings();

			WeightText = WeightText?.Trim();
			SumText = SumText?.Trim();
		}

		public bool EqualsModelWithoutWorkAndRework(FullBackDataController obj, DocumentTemplateType documentTemplate)
		{
			return model.EqualsWithoutWorkAndRework(obj.model, documentTemplate);
		}

		public void EnableActionsStacking()
		{
			model.EnableActionsStacking();
		}

		public void DisableActionsStacking()
		{
			model.DisableActionsStacking();
		}

		public void SetSumTextChangesWithLink(string sumText, params IUndoRedoAction[] links)
		{
			model.SetSumTextChangesWithLink(sumText, links);
		}
	}
}
