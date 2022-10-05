using System;

namespace Db.Context.HumanPart
{
	public class StreetType : IDbObject
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string ShortName { get; set; }

		public void Set(StreetType other)
		{
			Name = other.Name;
			ShortName = other.ShortName;
		}

		public void Set(IDbObject other)
		{
			if (other is StreetType obj)
				Set(obj);
			else
				throw new InvalidOperationException();
		}
	}
}
