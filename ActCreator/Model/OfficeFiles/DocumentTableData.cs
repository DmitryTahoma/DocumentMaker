using ActCreator.Model.Template;
using System.Collections;
using System.Collections.Generic;

namespace ActCreator.Model.OfficeFiles
{
    class DocumentTableData : IEnumerable<DocumentTableRowData>
    {
        private List<DocumentTableRowData> rows;

        public DocumentTableData(IEnumerable<BackDataModel> backModels, DocumentTemplateType templateType)
        {
            rows = new List<DocumentTableRowData>();

            foreach (BackDataModel model in backModels)
            {
                rows.Add(new DocumentTableRowData(model, templateType));
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
