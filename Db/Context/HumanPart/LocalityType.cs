using System;

namespace Db.Context.HumanPart
{
	public class LocalityType : IDbObject
	{
		public LocalityType() { }

		public LocalityType(LocalityType other)
		{
			Id = other.Id;
			Name = other.Name;
			ShortName = other.ShortName;
		}

		public int Id { get; set; }
		public string Name { get; set; }
		public string ShortName { get; set; }

		public void Set(LocalityType other)
		{
			Name = other.Name;
			ShortName = other.ShortName;
		}

		public void Set(IDbObject other)
		{
			if (other is LocalityType obj)
				Set(obj);
			else
				throw new InvalidOperationException();
		}
	}
}
