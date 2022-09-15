using Dml;
using Dml.Model;
using Dml.Model.Back;
using Dml.Model.Template;
using Dml.UndoRedo;
using DocumentMaker.Model.Back;

namespace DocumentMaker.Model.Controls
{
	public class FullBackDataModel : BaseBackDataModel
	{
		private readonly PropertySetActionProvider<FullBackDataModel, string> sumTextActionProvider = new PropertySetActionProvider<FullBackDataModel, string>(x => x.SumText);

		private string sumText;

		public FullBackDataModel() : base()
		{
			WorkTypesList = new ObservableRangeCollection<WorkObject>();
		}

		public string WeightText { get; set; }
		public double Weight { get; set; }
		public string SumText
		{
			get => sumText;
			set
			{
				if (actionsStack?.ActionsStackingEnabled ?? false) actionsStack.Push(sumTextActionProvider.CreateAction(this, value));
				sumText = value;
			}
		}
		public uint WorkObjectId { get; set; }
		public bool IsOtherType { get; set; }
		public ObservableRangeCollection<WorkObject> WorkTypesList { get; private set; }

		public bool EqualsWithoutWork(FullBackDataModel obj, DocumentTemplateType documentTemplate)
		{
			return Type == obj.Type &&
				(
					(
						Type != BackType.Other &&
						EpisodeNumberText == obj.EpisodeNumberText &&
						BackName == obj.BackName &&
						GameName == obj.GameName &&
						IsRework == obj.IsRework &&
						(
							documentTemplate != DocumentTemplateType.Painter ||
							IsSketch == obj.IsSketch
						) &&
						(
							(
								Type != BackType.Regions &&
								Type != BackType.HogRegions
							) ||
							BackCountRegionsText == obj.BackCountRegionsText
						) &&
						(
							Type == BackType.Craft ||
							BackNumberText == obj.BackNumberText
						)
					) ||
					(
						Type == BackType.Other &&
						OtherText == obj.OtherText
					)
				)
			;
		}

		public bool EqualsWithoutWorkAndRework(FullBackDataModel obj, DocumentTemplateType documentTemplate)
		{
			return Type == obj.Type &&
				(
					(
						Type != BackType.Other &&
						EpisodeNumberText == obj.EpisodeNumberText &&
						BackName == obj.BackName &&
						GameName == obj.GameName &&
						(
							documentTemplate != DocumentTemplateType.Painter ||
							IsSketch == obj.IsSketch
						) &&
						(
							(
								Type != BackType.Regions &&
								Type != BackType.HogRegions
							) ||
							BackCountRegionsText == obj.BackCountRegionsText
						) &&
						(
							Type == BackType.Craft ||
							BackNumberText == obj.BackNumberText
						)
					) ||
					(
						Type == BackType.Other &&
						OtherText == obj.OtherText
					)
				)
			;
		}

		public void EnableActionsStacking()
		{
			if (actionsStack != null)
				actionsStack.ActionsStackingEnabled = true;
		}

		public void DisableActionsStacking()
		{
			if (actionsStack != null)
				actionsStack.ActionsStackingEnabled = false;
		}

		public void SetSumTextChangesWithLink(string sumText, params IUndoRedoAction[] links)
		{
			IUndoRedoAction action = sumTextActionProvider.CreateAction(this, sumText);
			foreach (IUndoRedoAction link in links)
			{
				action.AddLink(link);
			}
			actionsStack.AddLinkToLast(action);
			this.sumText = sumText;
		}
	}
}
