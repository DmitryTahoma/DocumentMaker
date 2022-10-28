namespace Db.Context.ActPart
{
	public class WorkType : IDbObject
	{
		public int Id { get; set; }
		public int? TemplateTypeId { get; set; }
		public TemplateType TemplateType { get; set; }
		public string Name { get; set; }
		public int? ActPartId { get; set; }
		public ActPart ActPart { get; set; }

		public void Set(IDbObject other)
		{
			throw new System.NotImplementedException();
		}
	}
}
