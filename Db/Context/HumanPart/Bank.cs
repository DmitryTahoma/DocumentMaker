using System;

namespace Db.Context.HumanPart
{
	public class Bank : IDbObject
	{
		public Bank() { }

		public Bank(Bank other)
		{
			Id = other.Id;
			Name = other.Name;
			IBT = other.IBT;
		}

		public int Id { get; set; }
		public string Name { get; set; }
		/// <summary>
		/// Inter-Branch Turnover (МФО Межфилиальный оборот)
		/// </summary>
		public int IBT { get; set; }

		public void Set(Bank other)
		{
			Name = other.Name;
			IBT = other.IBT;
		}

		public void Set(IDbObject other)
		{
			if (other is Bank obj)
				Set(obj);
			else
				throw new InvalidOperationException();
		}
	}
}
