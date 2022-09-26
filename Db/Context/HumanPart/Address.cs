namespace Db.Context.HumanPart
{
	public class Address
	{
		public int Id { get; set; }
		public int? LocalityTypeId { get; set; }
		public LocalityType LocalityType { get; set; }
		public string LocalityName { get; set; }
		public int? StreetTypeId { get; set; }
		public StreetType StreetType { get; set; }
		public string StreetName { get; set; }
		public string HouseNumber { get; set; }
		public string ApartmentNumber { get; set; }
	}
}
