namespace Dml.UndoRedo
{
	public interface IUndoRedoAction
	{
		void Redo();
		void Undo();
		void AddLink(IUndoRedoAction action);
	}
}
