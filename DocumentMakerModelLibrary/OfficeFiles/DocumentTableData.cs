using Dml.Model.Template;
using DocumentMakerModelLibrary.Controls;
using System.Collections;
using System.Collections.Generic;

namespace DocumentMakerModelLibrary.OfficeFiles
{
	internal class DocumentTableData : IEnumerable<DocumentTableRowData>
	{
		private readonly List<DocumentTableRowData> rows;

		public DocumentTableData(IEnumerable<FullBackDataModel> backModels, DocumentTemplateType templateType, bool isExportRework)
		{
			rows = new List<DocumentTableRowData>();

			foreach (FullBackDataModel model in backModels)
			{
				if (model.IsRework == isExportRework)
				{
					rows.Add(new DocumentTableRowData(model, templateType));
				}
			}
		}

		public DocumentTableData(IEnumerable<FullBackDataModel> backModels, DocumentMakerModel docModel)
		{
			rows = new List<DocumentTableRowData>();
			uint id = 1;
			foreach (FullBackDataModel model in backModels)
			{
				if (!model.IsRework)
				{
					rows.Add(new DocumentTableRowData(model, docModel, id));
					id++;
				}
			}

			foreach (FullBackDataModel model in backModels)
			{
				if (model.IsRework)
				{
					rows.Add(new DocumentTableRowData(model, docModel, id));
					id++;
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
