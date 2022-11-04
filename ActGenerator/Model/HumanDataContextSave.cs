using Dml.Model.Template;

namespace ActGenerator.Model
{
	public partial class ActGeneratorSession
	{ 
		public class HumanDataContextSave
		{
			public HumanDataContextSave() { }

			public HumanDataContextSave(HumanDataContext humanDataContext)
			{
				SumText = humanDataContext.SumText;
				TemplateType = humanDataContext.Template?.Type ?? DocumentTemplateType.Empty;
			}

			public string SumText { get; set; }
			public DocumentTemplateType TemplateType { get; set; }
			public int ContextId { get; set; }
		}
	}
}
