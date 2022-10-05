using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Mvvm
{
	public class ObservableRangeCollection<T> : ObservableCollection<T>
	{
		private bool suppressNotification = false;

		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			if (!suppressNotification)
				base.OnCollectionChanged(e);
		}

		public void AddRange(IEnumerable<T> list)
		{
			if (list == null)
				throw new ArgumentNullException("list");

			suppressNotification = true;

			foreach (T item in list)
			{
				Add(item);
			}
			suppressNotification = false;
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		public int RemoveAll(Predicate<T> match)
		{
			Stack<T> removed = new Stack<T>();
			foreach(T elem in this)
			{
				if(match(elem))
				{
					removed.Push(elem);
				}
			}
			int counter = removed.Count;
			while (removed.Count > 0)
			{
				Remove(removed.Pop());
			}
			return counter;
		}
	}
}
