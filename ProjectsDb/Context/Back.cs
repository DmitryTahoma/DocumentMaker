using System;
using System.Collections.Generic;

namespace ProjectsDb.Context
{
	public class Back : IDbObject
	{
		public int Id { get; set; }
		public int? BackTypeId { get; set; }
		public BackType BackType { get; set; }
		public string Name { get; set; }
		public int? BaseBackId { get; set; }
		public Back BaseBack { get; set; }
		public float Number { get; set; }
		public int? ProjectId { get; set; }
		public Project Project { get; set; }

		public List<CountRegions> Regions { get; set; }
		public List<Back> ChildBacks { get; set; }

		public void Set(Back obj)
		{
			Name = obj.Name;
			Number = obj.Number;
			if(obj.ProjectId != null)
			{
				ProjectId = obj.ProjectId;
			}
			if(obj.BackTypeId != null)
			{
				BackTypeId = obj.BackTypeId;
			}
			if(obj.BaseBackId != null)
			{
				BaseBackId = obj.BaseBackId;
			}
		}

		public void Set(IDbObject other)
		{
			if (other is Back obj)
				Set(obj);
			else
				throw new InvalidOperationException();
		}

		public override string ToString()
		{
			return Number.ToString() + ". " + Name;
		}
	}
}
