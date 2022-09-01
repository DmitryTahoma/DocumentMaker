using Dml.Model.Template;
using DocumentMaker.Model.Back;
using DocumentMaker.Model.Controls;
using System;
using System.Collections.Generic;

namespace DocumentMaker.Model.Algorithm
{
	internal static class BackDataRandomizer
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
				else if ((!pair.Key || !isCurrentBack) && current.EqualsWithoutWork(pair.Value, documentTemplate))
				{
					res.Remove(pair.Value.WorkTypesList[(int)pair.Value.WorkObjectId]);
				}
			}

			return res;
		}
	}
}
