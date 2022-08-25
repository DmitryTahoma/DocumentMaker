using Dml.Model.Files;
using Dml.Model.Session;
using DocumentMaker.Model.Controls;
using System;
using System.Collections.Generic;

namespace DocumentMaker.Model.Files
{
	public class DmxFile : BaseDmxFile
	{
		private List<FullBackDataModel> backDataModels;

		public DmxFile(string path) : base(path)
		{
			backDataModels = null;
		}

		public string ActSum { get; set; }
		public IList<FullBackDataModel> BackDataModels { get => backDataModels; }

		public override void Load()
		{
			XmlLoader loader = new XmlLoader();
			if (loader.TryLoad(FullName))
			{
				loader.SetLoadedProperties(this);

				backDataModels = new List<FullBackDataModel>();
				loader.SetLoadedListProperties(backDataModels);
				SetDefaultSums();

				Loaded = true;
			}
		}

		public void SetLoadedBackData(IList<FullBackDataModel> backDataModels)
		{
			this.backDataModels = new List<FullBackDataModel>(backDataModels);
			Loaded = true;
		}

		public void AddBackModel(FullBackDataModel backDataModel)
		{
			backDataModels.Add(backDataModel);
		}

		private void SetDefaultSums()
		{
			foreach (FullBackDataModel model in backDataModels)
			{
				if (model.SumText == null)
				{
					model.SumText = model.SpentTimeText + "00";
				}
			}
		}

		public void RemoveBackModel(FullBackDataModel model)
		{
			backDataModels.Remove(model);
		}

		public void RemoveAllBackModel(Predicate<FullBackDataModel> match)
		{
			backDataModels.RemoveAll(match);
		}
	}
}
