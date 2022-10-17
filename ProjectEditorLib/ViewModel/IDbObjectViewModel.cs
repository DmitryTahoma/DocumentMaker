using Db.Context;

namespace ProjectEditorLib.ViewModel
{
	public interface IDbObjectViewModel
	{
		IDbObject UpdateContext(IDbObject dbObject);
	}
}
