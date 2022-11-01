namespace DocumentMakerModelLibrary.Files
{
	public class DcmkFile : DmxFile
	{
		public static new string Extension => ".dcmk";

		public DcmkFile(string path) : base(path) { }

		public override void Load()
		{
			TryLoadWithBacks();
		}
	}
}
