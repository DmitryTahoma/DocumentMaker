using Dml;
using ProjectEditorLib.View;
using ProjectEditorLib.ViewModel;
using System;

namespace ProjectEditorLib.Model
{
	internal class ComparableTreeItemHeader : TreeItemHeader, IComparable
	{
		NaturalStringComparer naturalStringComparer = new NaturalStringComparer();

		public int CompareTo(object obj)
		{
			if (obj is TreeItemHeader other)
			{
				ProjectNode thisModel = ((TreeItemHeaderViewModel)DataContext).GetModel();
				ProjectNode otherModel = ((TreeItemHeaderViewModel)other.DataContext).GetModel();
				int comparedType = thisModel.Type.CompareTo(otherModel.Type);

				return comparedType != 0
					? comparedType
					: naturalStringComparer.Compare(thisModel, otherModel);
			}

			throw new ArgumentException();
		}
	}
}
