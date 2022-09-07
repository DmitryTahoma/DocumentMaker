using System.Collections.Generic;

namespace Dml.UndoRedo
{
	public abstract class BaseUndoRedoAction : IUndoRedoAction
	{
		protected List<IUndoRedoAction> chain;

		public BaseUndoRedoAction()
		{
			chain = new List<IUndoRedoAction>();
		}

		public virtual void Redo()
		{
			foreach(IUndoRedoAction link in chain)
			{
				link.Redo();
			}
		}

		public virtual void Undo()
		{
			foreach(IUndoRedoAction link in chain)
			{
				link.Undo();
			}
		}

		public virtual void AddLink(IUndoRedoAction action)
		{
			chain.Add(action);
		}
	}
}
