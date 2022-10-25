using System.IO;
using System.Reflection;

namespace Dml
{
	public static class PathHelper
	{
		public static string ExecutingAssemblyPath { get; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
	}
}
