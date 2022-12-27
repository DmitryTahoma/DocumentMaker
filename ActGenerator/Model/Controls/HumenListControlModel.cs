using Dml;
using DocumentMakerModelLibrary;
using System.Collections.Generic;
using System.IO;

namespace ActGenerator.Model.Controls
{
	class HumenListControlModel
	{
		readonly string reworkWorkTypesPath = Path.Combine(PathHelper.ExecutingAssemblyPath, "SupportTypes.xlsx");

		public IEnumerable<FullDocumentTemplate> DocumentTemplatesList { get; } = new DocumentMakerModel().DocumentTemplatesList;

		public void LoadWorkTypes()
		{
			foreach(FullDocumentTemplate documentTemplate in DocumentTemplatesList)
			{
				documentTemplate.LoadReworkWorkTypesList(reworkWorkTypesPath);
			}
		}
	}
}
