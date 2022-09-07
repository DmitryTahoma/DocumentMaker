using System;

namespace Dml.UndoRedo
{
	public class UndoRedoLink : BaseUndoRedoAction, IUndoRedoAction
	{
		private readonly Action<object> redo;
		private readonly Action<object> undo;

		public UndoRedoLink(Action action)
		{
			redo = ConvertAction(action);
			undo = ConvertAction(action);
		}

		public UndoRedoLink(Action<object> action)
		{
			redo = action;
			undo = action;
		}

		public UndoRedoLink(Action redo, Action undo)
		{
			this.redo = ConvertAction(redo);
			this.undo = ConvertAction(undo);
		}

		public UndoRedoLink(Action<object> redo, Action<object> undo)
		{
			this.redo = redo;
			this.undo = undo;
		}

		public object Data { get; set; } = null;

		public override void Redo()
		{
			redo(Data);
			base.Redo();
		}

		public override void Undo()
		{
			undo(Data);
			base.Undo();
		}

		private Action<object> ConvertAction(Action action) => (d) => action();
	}
}
