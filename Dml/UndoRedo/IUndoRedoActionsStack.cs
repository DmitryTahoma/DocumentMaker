namespace Dml.UndoRedo
{
	public interface IUndoRedoActionsStack
	{
		bool ActionsStackingEnabled { get; set; }
		bool CanRedo { get; }
		bool CanUndo { get; }
		int MaxCapacity { get; }

		void Push(IUndoRedoAction action);
		void Redo();
		void Undo();
		void AddLinkToLast(IUndoRedoAction action);
		void RemoveActionsWithTarget<TObj>(TObj target);
	}
}
