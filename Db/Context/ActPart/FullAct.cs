﻿using System;

namespace Db.Context.ActPart
{
	public class FullAct : IDbObject
	{
		public int Id { get; set; }
		public int? ActId { get; set; }
		public Act Act { get; set; }
		public DateTime TechnicalTaskDate { get; set; }
		public DateTime ActDate { get; set; }
		public int TechnicalTaskNumber { get; set; }
		public int ActSum { get; set; }

		public void Set(IDbObject other)
		{
			throw new NotImplementedException();
		}
	}
}
