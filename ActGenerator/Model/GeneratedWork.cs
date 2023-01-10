using DocumentMakerModelLibrary.Back;
using ProjectsDb.Context;

namespace ActGenerator.Model
{
	class GeneratedWork
	{
		public WorkObject WorkObject { get; set; }
		public IDbObject Project { get; set; }
		public Back Back { get; set; }
	}
}
