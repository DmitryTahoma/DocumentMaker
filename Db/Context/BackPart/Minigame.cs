namespace Db.Context.BackPart
{
	public class Minigame
	{
		public int Id { get; set; }
		public int? BackId { get; set; }
		public Back Back { get; set; }
		public int Number { get; set; }
	}
}
