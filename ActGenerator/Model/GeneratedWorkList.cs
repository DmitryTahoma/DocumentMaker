using DocumentMakerModelLibrary;
using ProjectsDb.Context;
using System.Collections.Generic;
using System.Linq;

namespace ActGenerator.Model
{
	class GeneratedWorkList
	{
		Dictionary<FullDocumentTemplate, List<GeneratedWork>> generatedWorks = new Dictionary<FullDocumentTemplate, List<GeneratedWork>>();

		public IDbObject Project { get; set; } = null;
		public IDictionary<FullDocumentTemplate, List<GeneratedWork>> GeneratedWorks { get => generatedWorks; }

		public void AddGeneratedWork(GeneratedWork generatedWork)
		{
			if (generatedWorks.ContainsKey(generatedWork.DocumentTemplate))
			{
				generatedWorks[generatedWork.DocumentTemplate].Add(generatedWork);
			}
			else
			{
				generatedWorks.Add(generatedWork.DocumentTemplate, new List<GeneratedWork> { generatedWork });
			}
		}

		public void InsertGeneratedWork(int index, GeneratedWork generatedWork)
		{
			if (generatedWorks.ContainsKey(generatedWork.DocumentTemplate))
			{
				generatedWorks[generatedWork.DocumentTemplate].Insert(index, generatedWork);
			}
			else
			{
				generatedWorks.Add(generatedWork.DocumentTemplate, new List<GeneratedWork> { generatedWork });
			}
		}

		public void RemoveGeneratedWork(GeneratedWork generatedWork)
		{
			if (generatedWorks.ContainsKey(generatedWork.DocumentTemplate))
			{
				generatedWorks[generatedWork.DocumentTemplate].Remove(generatedWork);
			}
		}

		public void CopyWorks(IDictionary<FullDocumentTemplate, List<GeneratedWork>> worksSource)
		{
			generatedWorks = new Dictionary<FullDocumentTemplate, List<GeneratedWork>>(worksSource);
		}

		public GeneratedWorkList Clone()
		{
			return new GeneratedWorkList
			{
				Project = Project,
				generatedWorks = new Dictionary<FullDocumentTemplate, List<GeneratedWork>>
					(generatedWorks
						.Select(x => new KeyValuePair<FullDocumentTemplate, List<GeneratedWork>>(x.Key, new List<GeneratedWork>(x.Value)))
						.ToDictionary(key => key.Key, value => value.Value)),
			};
		}
	}
}
