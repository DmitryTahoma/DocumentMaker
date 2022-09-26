namespace Db.Context.ActPart
{
	public class FullWork
	{
		public int Id { get; set; }
		public int? WorkId { get; set; }
		public Work Work { get; set; }
		public int Sum { get; set; }
	}
}
