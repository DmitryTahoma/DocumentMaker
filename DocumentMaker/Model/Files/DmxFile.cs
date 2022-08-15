﻿using Dml.Model.Files;
using Dml.Model.Session;
using DocumentMaker.Model.Controls;
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

		public IList<FullBackDataModel> BackDataModels { get => backDataModels; }

		public override void Load()
		{
			XmlLoader loader = new XmlLoader();
			if (loader.TryLoad(FullName))
			{
				loader.SetLoadedProperties(this);

				backDataModels = new List<FullBackDataModel>();
				loader.SetLoadedListProperties(backDataModels);

				Loaded = true;
			}
		}

		public void SetLoadedBackData(IList<FullBackDataModel> backDataModels)
		{
			this.backDataModels = new List<FullBackDataModel>(backDataModels);
			Loaded = true;
		}
	}
}
