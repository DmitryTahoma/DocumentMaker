namespace Dml.UndoRedo
{
	public interface ITargetValueUndoRedoAction<TObj, TProp> : ITargetUndoRedoAction<TObj>, ITargetValueUndoRedoAction, ITargetUndoRedoAction, IUndoRedoAction
	{
		TProp OldValue { get; }
		TProp NewValue { get; }
	}
}
