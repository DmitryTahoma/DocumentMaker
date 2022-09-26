namespace Db.Context.BackPart
{
	public class CountRegions
	{
		public int Id { get; set; }
		public int? BackId { get; set; }
		public Back Back { get; set; }
		public int Count { get; set; }
	}
}
