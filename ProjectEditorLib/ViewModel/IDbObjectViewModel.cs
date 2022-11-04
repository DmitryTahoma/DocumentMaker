using ProjectsDb.Context;

namespace ProjectEditorLib.ViewModel
{
	public interface IDbObjectViewModel
	{
		bool HaveUnsavedChanges { get; set; }
		IDbObject UpdateContext(IDbObject dbObject);
		void SetFromContext(IDbObject dbObject);
		bool CancelChanges();
	}
}
