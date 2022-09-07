namespace Dml.UndoRedo
{
	public interface ITargetUndoRedoAction<TObj> : IUndoRedoAction
	{
		TObj Target { get; }
	}
}
