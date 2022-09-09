namespace Dml.UndoRedo
{
	public interface ITargetValueUndoRedoAction : ITargetUndoRedoAction
	{
		object GetOldValue();
		object GetNewValue();
		void SetNewValue(object v);
	}
}
