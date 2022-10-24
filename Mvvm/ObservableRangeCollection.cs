using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Mvvm
{
	public class ObservableRangeCollection<T> : ObservableCollection<T>
	{
		public bool SuppressingNotifications { get; set; } = false;

		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			if (!SuppressingNotifications)
				base.OnCollectionChanged(e);
		}

		public void AddRange(IEnumerable<T> list)
		{
			if (list == null)
				throw new ArgumentNullException("list");

			bool suppressingNotifications = SuppressingNotifications;
			SuppressingNotifications = true;

			foreach (T item in list)
			{
				Add(item);
			}
			SuppressingNotifications = suppressingNotifications;
			UpdateCollection();
		}

		public void UpdateCollection()
		{
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		public void RemoveRange(IEnumerable<T> list)
		{
			if (list == null)
				throw new ArgumentNullException("list");

			bool suppressingNotifications = SuppressingNotifications;
			SuppressingNotifications = true;

			foreach (T item in list)
			{
				Remove(item);
			}
			SuppressingNotifications = suppressingNotifications;
			UpdateCollection();
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
