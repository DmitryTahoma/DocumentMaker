using DocumentMakerModelLibrary;
using System.Collections.Generic;

namespace ActGenerator.Model.Controls
{
	class HumenListControlModel
	{
		public IEnumerable<FullDocumentTemplate> DocumentTemplatesList { get; } = new DocumentMakerModel().DocumentTemplatesList;
	}
}
