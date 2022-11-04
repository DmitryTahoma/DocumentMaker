using Dml.Model.Template;

namespace ActGenerator.Model
{
	public class HumanDataContext
	{
		public HumanDataContext()
		{
		}

		public string FullName { get => string.Empty; }
		public string SumText { get; set; }
		public DocumentTemplate Template { get; set; }
	}
}
