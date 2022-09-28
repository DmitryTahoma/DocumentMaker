namespace Db.Context.HumanPart
{
	public class StreetType
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string ShortName { get; set; }

		public void Set(StreetType other)
		{
			Name = other.Name;
			ShortName = other.ShortName;
		}
	}
}
