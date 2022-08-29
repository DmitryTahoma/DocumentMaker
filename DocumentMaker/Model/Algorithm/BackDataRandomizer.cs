using Dml.Model.Back;
using Dml.Model.Template;
using DocumentMaker.Model.Back;
using DocumentMaker.Model.Controls;
using System;
using System.Collections.Generic;

namespace DocumentMaker.Model.Algorithm
{
	static class BackDataRandomizer
	{
		public static void ByWorkTypeName(IEnumerable<KeyValuePair<bool, FullBackDataModel>> backDatas, DocumentTemplateType documentTemplate)
		{
			List<KeyValuePair<bool, FullBackDataModel>> data = new List<KeyValuePair<bool, FullBackDataModel>>(backDatas);
			Random random = new Random();

			foreach (KeyValuePair<bool, FullBackDataModel> pair in data)
			{
				if (pair.Key)
				{
					IList<WorkObject> enableWorks = GetEnableWorks(pair.Value, data, documentTemplate);
					pair.Value.WorkObjectId = enableWorks.Count > 0 ? enableWorks[random.Next(enableWorks.Count)].Id : 0;
				}
			}
		}

		private static IList<WorkObject> GetEnableWorks(FullBackDataModel current, List<KeyValuePair<bool, FullBackDataModel>> elems, DocumentTemplateType documentTemplate)
		{
			List<WorkObject> res = new List<WorkObject>(current.WorkTypesList);

			bool isCurrentBack = false;
			foreach (KeyValuePair<bool, FullBackDataModel> pair in elems)
			{
				if (pair.Value == current)
				{
					isCurrentBack = true;
				}
				else if ((!pair.Key || !isCurrentBack) && EqualsBackDataModels(current, pair.Value, documentTemplate))
				{
					res.Remove(pair.Value.WorkTypesList[(int)pair.Value.WorkObjectId]);
				}
			}

			return res;
		}

		private static bool EqualsBackDataModels(FullBackDataModel obj1, FullBackDataModel obj2, DocumentTemplateType documentTemplate)
		{
			return obj1.Type == obj2.Type &&
				(
					(
						obj1.Type != BackType.Other &&
						obj1.EpisodeNumberText == obj2.EpisodeNumberText &&
						obj1.BackName == obj2.BackName &&
						obj1.GameName == obj2.GameName &&
						obj1.IsRework == obj2.IsRework &&
						(
							documentTemplate != DocumentTemplateType.Painter ||
							obj1.IsSketch == obj2.IsSketch
						) &&
						(
							(
								obj1.Type != BackType.Regions &&
								obj1.Type != BackType.HogRegions
							) ||
							obj1.BackCountRegionsText == obj2.BackCountRegionsText
						) &&
						(
							obj1.Type == BackType.Craft ||
							obj1.BackNumberText == obj2.BackNumberText
						)
					) ||
					(
						obj1.Type == BackType.Other &&
						obj1.OtherText == obj2.OtherText
					)
				)
			;
		}
	}
}
