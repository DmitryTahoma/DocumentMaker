using Dml;
using DocumentMakerModelLibrary.OfficeFiles;
using DocumentMakerModelLibrary.OfficeFiles.Human;
using System.IO;

namespace ActGenerator.Model.Dialogs
{
	class AddHumanDialogModel
	{
		private const string humansFile = "HumanData.xlsx";

		public ObservableRangeCollection<HumanData> LoadHumans()
		{
			ObservableRangeCollection<HumanData> humen = new ObservableRangeCollection<HumanData>();
			XlsxLoader loader = new XlsxLoader();
			loader.LoadHumans(Path.Combine(PathHelper.ExecutingAssemblyPath, humansFile), humen);
			return humen;
		}
	}
}
