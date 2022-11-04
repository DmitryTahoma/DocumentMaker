namespace ProjectsDb.Context
{
	public class BackType : IDbObject
	{
		public int Id { get; set; }
		public string Name { get; set; }

		public void Set(IDbObject other)
		{
			throw new System.NotImplementedException();
		}
	}
}
