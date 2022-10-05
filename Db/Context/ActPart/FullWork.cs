namespace Db.Context.ActPart
{
	public class FullWork : IDbObject
	{
		public int Id { get; set; }
		public int? WorkId { get; set; }
		public Work Work { get; set; }
		public int Sum { get; set; }

		public void Set(IDbObject other)
		{
			throw new System.NotImplementedException();
		}
	}
}
