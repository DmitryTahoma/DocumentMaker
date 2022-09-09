using System;

namespace Dml.UndoRedo
{
	public class PropertySetAction<TObj, TProp> : BaseUndoRedoAction, ITargetValueUndoRedoAction<TObj, TProp>
	{
		private readonly Action<TObj, TProp> setter;

		public PropertySetAction(Action<TObj, TProp> setter, TObj target, TProp oldValue, TProp newValue)
		{
			this.setter = setter;

			Target = target;
			OldValue = oldValue;
			NewValue = newValue;
		}

		public TObj Target { get; }
		public TProp OldValue { get; }
		public TProp NewValue { get; private set; }

		public object GetOldValue()
		{
			return OldValue;
		}

		public object GetNewValue()
		{
			return NewValue;
		}

		public void SetNewValue(object v)
		{
			NewValue = (TProp)v;
		}

		public object GetTarget()
		{
			return Target;
		}

		public override void Redo()
		{
			setter(Target, NewValue);
			base.Redo();
		}

		public override void Undo()
		{
			setter(Target, OldValue);
			base.Undo();
		}
	}
}
