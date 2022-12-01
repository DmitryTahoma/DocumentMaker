using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Dml
{
	public class NaturalStringComparer : IComparer, IComparer<object>, IComparer<string>
	{
		[DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
		public static extern int StrCmpLogicalW(string a, string b);

		public int Compare(object a, object b)
		{
			return Compare(a?.ToString(), b?.ToString());
		}

		public int Compare(string a, string b)
		{
			return StrCmpLogicalW(a, b);
		}
	}
}
