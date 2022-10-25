using Db.Context.HumanPart;
using Dml.Model.Template;

namespace ActGenerator.Model
{
	class HumanDataContext
	{
		public HumanDataContext(Human context)
		{
			Context = context;
		}

		public string FullName { get => Context.FullName; }
		public string SumText { get; set; }
		public DocumentTemplate Template { get; set; }
		public Human Context { get; private set; }
	}
}
