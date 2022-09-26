namespace Db.Context.ActPart
{
	public class Regions
	{
		public int Id { get; set; }
		public int? WorkId { get; set; }
		public Work Work { get; set; }
		public int Number { get; set; }
	}
}
