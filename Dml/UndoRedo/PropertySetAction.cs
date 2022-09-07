using System;

namespace Dml.UndoRedo
{
	public class PropertySetAction<TObj, TProp> : BaseUndoRedoAction, ITargetUndoRedoAction<TObj>
	{
		private Action<TObj, TProp> setter;
		private TProp oldValue;
		private TProp newValue;

		public PropertySetAction(Action<TObj, TProp> setter, TObj target, TProp oldValue, TProp newValue)
		{
			this.setter = setter;
			this.oldValue = oldValue;
			this.newValue = newValue;

			Target = target;
		}

		public TObj Target { get; }

		public override void Redo()
		{
			setter(Target, newValue);
			base.Redo();
		}

		public override void Undo()
		{
			setter(Target, oldValue);
			base.Undo();
		}
	}
}
