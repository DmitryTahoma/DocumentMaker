using Db.Context.ActPart;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Db.Context.BackPart
{
	public class Back : IDbObject
	{
		public int Id { get; set; }
		public int? EpisodeId { get; set; }
		public Episode Episode { get; set; }
		public int? BackTypeId { get; set; }
		public BackType BackType { get; set; }
		public string Name { get; set; }
		public int? BaseBackId { get; set; }
		public Back BaseBack { get; set; }
		public float Number { get; set; }

		[InverseProperty("Back")]
		public List<CountRegions> Regions { get; set; }
		[InverseProperty("BaseBack")]
		public List<Back> ChildBacks { get; set; }
		[InverseProperty("Back")]
		public List<WorkBackAdapter> WorkBackAdapters { get; set; }

		public void Set(Back obj)
		{
			Name = obj.Name;
			Number = obj.Number;
			if(obj.EpisodeId != null)
			{
				EpisodeId = obj.EpisodeId;
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
