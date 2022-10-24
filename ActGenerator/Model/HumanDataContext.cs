using Dml.Model.Template;

namespace ActGenerator.Model
{
	class HumanDataContext
	{
		public HumanDataContext(string fullName)
		{
			FullName = fullName;
		}

		public string FullName { get; private set; }
		public string SumText { get; set; }
		public DocumentTemplate Template { get; set; }
	}
}
