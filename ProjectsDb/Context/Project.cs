using System;
using System.Collections.Generic;

namespace ProjectsDb.Context
{
	public class Project : IDbObject
	{
		public int Id { get; set; }
		public string Name { get; set; }

		public List<AlternativeProjectName> AlternativeNames { get; set; }
		public List<Back> Backs { get; set; }

		public void Set(Project obj)
		{
			Name = obj.Name;
		}

		public void Set(IDbObject other)
		{
			if (other is Project obj)
				Set(obj);
			else
				throw new InvalidOperationException();
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
