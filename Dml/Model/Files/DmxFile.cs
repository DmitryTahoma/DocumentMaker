using System.IO;

namespace Dml.Model.Files
{
	public class DmxFile
	{
		public static string Extension => ".dmx";

		public DmxFile(string path)
		{
			FullName = path;
		}

		public string FullName { get; private set; }
		public string Name => Path.GetFileName(FullName);

		public override string ToString()
		{
			return Name;
		}
	}
}
