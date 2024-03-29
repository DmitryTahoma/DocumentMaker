﻿using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectsDb.Context
{
	public class CountRegions : IDbObject
	{
		public int Id { get; set; }
		public int? BackId { get; set; }
		public Back Back { get; set; }
		public int Count { get; set; }
		[Column(TypeName = "date")]
		public DateTime? DeletionDate { get; set; }

		public void Set(CountRegions obj)
		{
			Count = obj.Count;
			if(obj.BackId != null)
			{
				BackId = obj.BackId;
			}
		}

		public void Set(IDbObject other)
		{
			if (other is CountRegions obj)
				Set(obj);
			else
				throw new InvalidOperationException();
		}
	}
}
