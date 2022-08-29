using Dml.Model.Back;
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
		public bool NeedUpdateSum { get; set; }
		public IList<FullBackDataModel> BackDataModels { get => backDataModels; }

		public override void Load()
		{
			XmlLoader loader = new XmlLoader();
			if (loader.TryLoad(FullName))
			{
				loader.SetLoadedProperties(this);

				backDataModels = new List<FullBackDataModel>();
				loader.SetLoadedListProperties(backDataModels);
				foreach (FullBackDataModel model in backDataModels)
				{
					if (model.Type == BackType.Other)
					{
						model.IsOtherType = true;
					}
					model.SumText = "0";
				}
				NeedUpdateSum = true;
				SetDefaultWeights();

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

		private void SetDefaultWeights()
		{
			double totalTime = 0;
			List<double> times = new List<double>();

			foreach (FullBackDataModel model in backDataModels)
			{
				int time = 0;
				if (int.TryParse(model.SpentTimeText, out int spentTime))
				{
					time = spentTime;
				}
				totalTime += time;
				times.Add(time);
			}

			IEnumerator<double> timesEnum = times.GetEnumerator();
			IEnumerator<FullBackDataModel> backDataModelsEnum = backDataModels.GetEnumerator();

			while (timesEnum.MoveNext() && backDataModelsEnum.MoveNext())
			{
				backDataModelsEnum.Current.Weight = timesEnum.Current / totalTime;
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
