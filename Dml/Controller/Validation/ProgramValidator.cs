using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Dml.Controller.Validation
{
	public static class ProgramValidator
	{
		public static bool ValidateExistsFiles(List<string> files)
		{
			string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			for (int i = 0; i < files.Count; ++i)
			{
				if (File.Exists(Path.Combine(path, files[i])))
				{
					files.Remove(files[i]);
					--i;
				}
			}

			return files.Count == 0;
		}
	}
}
