using Dml;
using Dml.Model;
using Dml.Model.Back;
using Dml.Model.Template;
using DocumentMaker.Model.Back;

namespace DocumentMaker.Model.Controls
{
	public class FullBackDataModel : BaseBackDataModel
	{
		public FullBackDataModel() : base()
		{
			WorkTypesList = new ObservableRangeCollection<WorkObject>();
		}

		public string WeightText { get; set; }
		public double Weight { get; set; }
		public string SumText { get; set; }
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
	}
}
