using System;

namespace Db.Context.HumanPart
{
	public class Address : IDbObject
	{
		public Address() { }

		public Address(Address other)
		{
			Id = other.Id;
			LocalityTypeId = other.LocalityTypeId;
			if (other.LocalityType != null) LocalityType = new LocalityType(other.LocalityType);
			LocalityName = other.LocalityName;
			StreetTypeId = other.StreetTypeId;
			if (other.StreetType != null) StreetType = new StreetType(other.StreetType);
			StreetName = other.StreetName;
			HouseNumber = other.HouseNumber;
			ApartmentNumber = other.ApartmentNumber;
		}

		public int Id { get; set; }
		public int? LocalityTypeId { get; set; }
		public LocalityType LocalityType { get; set; }
		public string LocalityName { get; set; }
		public int? StreetTypeId { get; set; }
		public StreetType StreetType { get; set; }
		public string StreetName { get; set; }
		public string HouseNumber { get; set; }
		public string ApartmentNumber { get; set; }

		public void Set(Address obj)
		{
			LocalityName = obj.LocalityName;
			StreetName = obj.StreetName;
			HouseNumber = obj.HouseNumber;
			ApartmentNumber = obj.ApartmentNumber;
			if (obj.LocalityTypeId != null)
			{
				LocalityTypeId = obj.LocalityTypeId;
			}
			if (obj.StreetTypeId != null)
			{
				StreetTypeId = obj.StreetTypeId;
			}
		}

		public void Set(IDbObject other)
		{
			if (other is Address obj)
				Set(obj);
			else
				throw new InvalidOperationException();
		}
	}
}
