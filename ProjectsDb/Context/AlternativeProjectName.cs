using System;

namespace ProjectsDb.Context
{
	public class AlternativeProjectName : IDbObject
	{
		public int Id { get; set; }
		public int? ProjectId { get; set; }
		public Project Project { get; set; }
		public string Name { get; set; }

		public void Set(AlternativeProjectName obj)
		{
			Name = obj.Name;
			if(obj.ProjectId != null)
			{
				ProjectId = obj.ProjectId;
			}
		}

		public void Set(IDbObject other)
		{
			if (other is AlternativeProjectName obj)
				Set(obj);
			else
				throw new InvalidOperationException();
		}
	}
}
