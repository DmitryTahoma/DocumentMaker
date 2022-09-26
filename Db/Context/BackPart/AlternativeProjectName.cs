namespace Db.Context.BackPart
{
	public class AlternativeProjectName
	{
		public int Id { get; set; }
		public int? ProjectId { get; set; }
		public Project Project { get; set; }
		public string Name { get; set; }
	}
}
