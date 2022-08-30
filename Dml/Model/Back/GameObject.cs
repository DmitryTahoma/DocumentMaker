using System.Collections.Generic;

namespace Dml.Model.Back
{
	public class GameObject
	{
		private List<string> episodes;

		public GameObject()
		{
			episodes = null;
		}

		public string Name { get; set; }

		public bool HaveEpisodes { get => episodes != null && episodes.Count > 0; }

		public IList<string> Episodes { get => episodes; }

		public void SetEpisodes(uint count)
		{
			if (episodes == null) episodes = new List<string>();
			episodes.Clear();

			for (uint i = 1; i <= count; ++i)
				episodes.Add(i.ToString());
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
