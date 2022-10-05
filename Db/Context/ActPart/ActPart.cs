namespace Db.Context.ActPart
{
	public class ActPart : IDbObject
	{
		public int Id { get; set; }
		public string Name { get; set; }

		public void Set(IDbObject other)
		{
			throw new System.NotImplementedException();
		}
	}
}
