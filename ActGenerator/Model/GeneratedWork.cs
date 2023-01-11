using DocumentMakerModelLibrary;
using DocumentMakerModelLibrary.Back;
using ProjectsDb.Context;
using System.Collections.Generic;

namespace ActGenerator.Model
{
	class GeneratedWork
	{
		public WorkObject WorkObject { get; set; }
		public Back Back { get; set; }
		public FullDocumentTemplate DocumentTemplate { get; set; }
		public List<int> Regions { get; set; }
		public bool BackUsed { get; set; } = false;

		public bool ContainWork()
		{
			return !BackUsed || (Regions != null && Regions.Count > 0);
		}
	}
}
