using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Dml.UndoRedo
{
	public class PropertySetActionProvider<TObj, TProp>
	{
		private readonly Func<TObj, TProp> getter;
		private readonly Action<TObj, TProp> setter;

		public PropertySetActionProvider(Expression<Func<TObj, TProp>> propertySelector)
		{
			getter = propertySelector.Compile();
			PropertyInfo property = (propertySelector.Body as MemberExpression).Member as PropertyInfo;
			setter = (Action<TObj, TProp>)Delegate.CreateDelegate(typeof(Action<TObj, TProp>), property.GetSetMethod());
		}

		public IUndoRedoAction CreateAction(TObj target, TProp newValue)
		{
			return new PropertySetAction<TObj, TProp>(setter, target, getter(target), newValue);
		}
	}
}
