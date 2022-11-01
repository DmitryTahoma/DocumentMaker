using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace DocumentMaker.Resources
{
	public static class ResourceUnloader
	{
		public static void Unload(string resourceName, string nearFullname)
		{
			byte[] resource = LoadResource(Assembly.GetExecutingAssembly().GetName().Name + ".Resources." + resourceName);
			using (FileStream fStream = new FileStream(nearFullname, FileMode.OpenOrCreate))
			{
				using (BinaryWriter writer = new BinaryWriter(fStream))
				{
					writer.Write(resource);
				}
			}
		}

		private static byte[] LoadResource(string name)
		{
			List<byte> bytes = new List<byte>();
			using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
			{
				bytes.Capacity = (int)stream.Length;
				using (BinaryReader reader = new BinaryReader(stream))
				{
					for (int i = 0; i < stream.Length; ++i)
					{
						bytes.Add(reader.ReadByte());
					}
				}
			}

			return bytes.ToArray();
		}
	}
}
