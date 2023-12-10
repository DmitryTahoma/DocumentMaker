using System.Collections.Generic;
using System.Linq;

namespace Dml.Model.Back
{
	public class GameObject
	{
		private List<string> episodes;
		private List<string> altName;

		public GameObject()
		{
			episodes = null;
			altName = new List<string>();
		}

		public string Name { get; set; }

		public bool HaveEpisodes { get => episodes != null && episodes.Count > 0; }

		public IList<string> Episodes { get => episodes; }
		public IList<string> AltName { get => altName; }

		public void SetEpisodes(uint count)
		{
			if (episodes == null) episodes = new List<string>();
			episodes.Clear();

			for (uint i = 1; i <= count; ++i)
				episodes.Add(i.ToString());

			if(episodes.Count != 0)
				episodes.Add("Всі");
		}

		public void AddAltName(string name)
		{
			if (altName == null) altName = new List<string>();

			if(!string.IsNullOrEmpty(name) && !altName.Contains(name))
				altName.Add(name);
		}

		public override string ToString()
		{
			return Name;
		}

		public bool HaveEpisode(string episodeName)
		{
			if (episodeName != null)
				return episodes.FirstOrDefault(x => x == episodeName) != null;
			return false;
		}
	}
}
