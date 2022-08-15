using Dml.Model.Back;
using Dml.Model.Session;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dml.Model
{
	public abstract class BaseBackDataModel
	{
		private readonly ObservableCollection<BackDataType> dataTypesList;
		private ObservableRangeCollection<string> gameNameList;

		public BaseBackDataModel()
		{
			dataTypesList = new ObservableCollection<BackDataType>
			{
				new BackDataType { Name = "Бек", Type = BackType.Back },
				new BackDataType { Name = "Регіони", Type = BackType.Regions },
				new BackDataType { Name = "Діалог", Type = BackType.Dialog },
				new BackDataType { Name = "Мініігра", Type = BackType.Mg },
				new BackDataType { Name = "Хог", Type = BackType.Hog },
				new BackDataType { Name = "Хог регіони", Type = BackType.HogRegions },
				new BackDataType { Name = "Крафт", Type = BackType.Craft },
				new BackDataType { Name = "Інше", Type = BackType.Other },
			};
			gameNameList = new ObservableRangeCollection<string>();
		}

		public uint Id { get; set; }
		public BackType Type { get; set; }
		public string BackNumberText { get; set; }
		public string BackName { get; set; }
		public string BackCountRegionsText { get; set; }
		public IList<string> GameNameList => gameNameList;
		public string GameName { get; set; }
		public bool IsRework { get; set; }
		public bool IsSketch { get; set; }
		public string SpentTimeText { get; set; }
		public string OtherText { get; set; }
		public IList<BackDataType> BackDataTypesList => dataTypesList;

		public virtual void LoadGameNames(string path)
		{
			XmlLoader loader = new XmlLoader();
			if(loader.TryLoad(path))
			{
				loader.SetLoadedGameNames(gameNameList);
			}
		}
	}
}
