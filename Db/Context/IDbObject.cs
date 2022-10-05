namespace Db.Context
{
	public interface IDbObject
	{
		int Id { get; }

		void Set(IDbObject other);
	}
}
