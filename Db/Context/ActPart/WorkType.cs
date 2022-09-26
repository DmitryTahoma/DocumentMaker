namespace Db.Context.ActPart
{
	public class WorkType
	{
		public int Id { get; set; }
		public int? TemplateTypeId { get; set; }
		public TemplateType TemplateType { get; set; }
		public string Name { get; set; }
	}
}
