using Dml.Model.Template;
using DocumentMaker.Model.Controls;
using System.Collections;
using System.Collections.Generic;

namespace DocumentMaker.Model.OfficeFiles
{
	internal class DocumentTableData : IEnumerable<DocumentTableRowData>
	{
		private readonly List<DocumentTableRowData> rows;

		public DocumentTableData(IEnumerable<FullBackDataModel> backModels, DocumentTemplateType templateType, bool isExportRework)
		{
			rows = new List<DocumentTableRowData>();

			foreach (FullBackDataModel model in backModels)
			{
				if(model.IsRework == isExportRework)
				{
					rows.Add(new DocumentTableRowData(model, templateType));
				}
			}
		}

		public IEnumerator<DocumentTableRowData> GetEnumerator()
		{
			return rows.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return rows.GetEnumerator();
		}
	}
}
