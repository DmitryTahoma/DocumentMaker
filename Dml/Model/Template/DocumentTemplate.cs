using Dml.Model.Back;
using System.Collections.Generic;

namespace Dml.Model.Template
{
	public class DocumentTemplate
	{
		protected readonly ObservableRangeCollection<BackDataType> dataTypesList;

		public DocumentTemplate(string name, DocumentTemplateType type)
		{
			dataTypesList = new ObservableRangeCollection<BackDataType>();

			Name = name;
			Type = type;

			FillDataTypesList(type);
		}

		public string Name { get; }
		public DocumentTemplateType Type { get; }
		public IList<BackDataType> DataTypesList => dataTypesList;

		public override string ToString()
		{
			return Name;
		}

		protected void FillDataTypesList(DocumentTemplateType type)
		{
			List<BackDataType> tempList = new List<BackDataType>();
			switch (type)
			{
				case DocumentTemplateType.Scripter:
					tempList.AddRange(new List<BackDataType>
					{
						new BackDataType { Name = "Бек", Type = BackType.Back },
						new BackDataType { Name = "Регіони", Type = BackType.Regions },
						new BackDataType { Name = "Діалог", Type = BackType.Dialog },
						new BackDataType { Name = "Мініігра", Type = BackType.Mg },
						new BackDataType { Name = "Хог", Type = BackType.Hog },
						new BackDataType { Name = "Хог регіони", Type = BackType.HogRegions },
						new BackDataType { Name = "Крафт", Type = BackType.Craft },
						new BackDataType { Name = "Iнтерфейс", Type = BackType.Interface },
					});
					break;
				case DocumentTemplateType.Cutter:
					tempList.AddRange(new List<BackDataType>
					{
						new BackDataType { Name = "Бек", Type = BackType.Back },
						new BackDataType { Name = "Регіони", Type = BackType.Regions },
						new BackDataType { Name = "Діалог", Type = BackType.Dialog },
						new BackDataType { Name = "Мініігра", Type = BackType.Mg },
						new BackDataType { Name = "Хог", Type = BackType.Hog },
						new BackDataType { Name = "Хог регіони", Type = BackType.HogRegions },
						new BackDataType { Name = "Крафт", Type = BackType.Craft },
						new BackDataType { Name = "Iнтерфейс", Type = BackType.Interface },
					});
					break;
				case DocumentTemplateType.Painter:
					tempList.AddRange(new List<BackDataType>
					{
						new BackDataType { Name = "Бек", Type = BackType.Back },
						new BackDataType { Name = "Регіони", Type = BackType.Regions },
						new BackDataType { Name = "Діалог", Type = BackType.Dialog },
						new BackDataType { Name = "Мініігра", Type = BackType.Mg },
						new BackDataType { Name = "Хог", Type = BackType.Hog },
						new BackDataType { Name = "Хог регіони", Type = BackType.HogRegions },
						new BackDataType { Name = "Крафт", Type = BackType.Craft },
						new BackDataType { Name = "Предмет", Type = BackType.Predmet },
						new BackDataType { Name = "Предмети на панель", Type = BackType.Predmets },
						new BackDataType { Name = "Морф", Type = BackType.Morf },
						new BackDataType { Name = "Коллекція", Type = BackType.Collection },
						new BackDataType { Name = "Персонаж", Type = BackType.Character },
						new BackDataType { Name = "Iнтерфейс", Type = BackType.Interface },
						new BackDataType { Name = "Маркетинг", Type = BackType.Marketing },
						new BackDataType { Name = "Відеоролик кадр", Type = BackType.VideoCadr },
						new BackDataType { Name = "Відео істоти", Type = BackType.VideoObject },
					});
					break;
				case DocumentTemplateType.Modeller:
					tempList.AddRange(new List<BackDataType>
					{
						new BackDataType { Name = "Бек", Type = BackType.Back },
						new BackDataType { Name = "Регіони", Type = BackType.Regions },
						new BackDataType { Name = "Діалог", Type = BackType.Dialog },
						new BackDataType { Name = "Мініігра", Type = BackType.Mg },
						new BackDataType { Name = "Хог", Type = BackType.Hog },
						new BackDataType { Name = "Хог регіони", Type = BackType.HogRegions },
						new BackDataType { Name = "Крафт", Type = BackType.Craft },
						new BackDataType { Name = "Предмет", Type = BackType.Predmet },
						new BackDataType { Name = "Предмети на панель", Type = BackType.Predmets },
						new BackDataType { Name = "Морф", Type = BackType.Morf },
						new BackDataType { Name = "Коллекція", Type = BackType.Collection },
						new BackDataType { Name = "Персонаж", Type = BackType.Character },
						new BackDataType { Name = "Iнтерфейс", Type = BackType.Interface },
						new BackDataType { Name = "Відеоролик кадр", Type = BackType.VideoCadr },
						new BackDataType { Name = "Відео істоти", Type = BackType.VideoObject },
					});
					break;
			}
			tempList.Add(new BackDataType { Name = "Інше", Type = BackType.Other });

			dataTypesList.Clear();
			dataTypesList.AddRange(tempList);
		}
	}
}
