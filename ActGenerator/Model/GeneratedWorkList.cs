using ProjectsDb.Context;
using System.Collections.Generic;

namespace ActGenerator.Model
{
	class GeneratedWorkList
	{
		public IDbObject Project { get; set; } = null;
		public List<GeneratedWork> GeneratedWorks { get; set; } = new List<GeneratedWork>();
	}
}
