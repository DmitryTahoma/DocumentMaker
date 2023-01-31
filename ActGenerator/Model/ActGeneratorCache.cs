using DocumentMakerModelLibrary;
using ProjectsDb.Context;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ActGenerator.Model
{
	class ActGeneratorCache
	{
		#region BackTypes Cache

		public List<BackType> BackTypes { get; set; } = null;

		#endregion

		#region Projects Cache

		List<Project> projectList = new List<Project>();
		List<AlternativeProjectName> alternativeProjectNamesList = new List<AlternativeProjectName>();

		public IDbObject GetProject(IDbObject dbObject)
		{
			return dbObject is Project
				? (IDbObject)FindProject(dbObject, projectList)
				: FindProject(dbObject, alternativeProjectNamesList);
		}

		public Project GetProject(Func<Project, bool> predicate)
		{
			return projectList
				.Union(alternativeProjectNamesList
					.Select(altName => altName.Project))
				.FirstOrDefault(predicate);
		}

		public bool TryGetProject(IDbObject dbObject, out IDbObject project)
		{
			project = GetProject(dbObject);
			return project != null;
		}

		public bool TryGetProject(Func<Project, bool> predicate, out Project project)
		{
			project = GetProject(predicate);
			return project != null;
		}

		public void SetProject(IDbObject dbObject)
		{
			if (dbObject is Project project)
				projectList.Add(project);
			else
				alternativeProjectNamesList.Add(dbObject as AlternativeProjectName);
		}

		public bool TryDuplicateProject(IDbObject dbObject, out IDbObject duplicatedProject)
		{
			Project project = UnpackProject(dbObject);

			Project findedProject = GetProject(x => x.Id == project.Id);
			if(findedProject != null)
			{
				project.Backs = findedProject.Backs;
				SetProject(dbObject);
				duplicatedProject = project;
				return true;
			}
			duplicatedProject = null;
			return false;
		}

		private T FindProject<T>(IDbObject source, List<T> list) where T : IDbObject
		{
			return list.FirstOrDefault(x => x.Id == source.Id);
		}

		#endregion

		#region GeneratedWorkList Cache

		List<GeneratedWorkList> generatedWorkLists = new List<GeneratedWorkList>();

		public GeneratedWorkList GetGeneratedWorkList(IDbObject dbObject)
		{
			return generatedWorkLists.FirstOrDefault(x => x.Project == dbObject);
		}

		public bool TryGetGeneratedWorkList(IDbObject dbObject, out GeneratedWorkList generatedWorkList)
		{
			generatedWorkList = GetGeneratedWorkList(dbObject);
			return generatedWorkList != null;
		}

		public void AddGeneratedWorkList(GeneratedWorkList generatedWorkList)
		{
			generatedWorkLists.Add(generatedWorkList);
		}

		public bool TryDuplicateGeneratedWorkList(IDbObject dbObject, out GeneratedWorkList generatedWorkList)
		{
			int projectId = GetProjectId(dbObject);

			GeneratedWorkList findedGeneratedWorkList = generatedWorkLists.FirstOrDefault(x => GetProjectId(x.Project) == projectId);
			if(findedGeneratedWorkList != null)
			{
				generatedWorkList = new GeneratedWorkList { Project = dbObject };
				generatedWorkList.CopyWorks(findedGeneratedWorkList.GeneratedWorks);
				AddGeneratedWorkList(generatedWorkList);
				return true;
			}
			generatedWorkList = null;
			return false;
		}

		public void UpdateGeneratedWorkList(GeneratedWorkList generatedWorkList)
		{
			IDbObject dbObject = generatedWorkList.Project;
			int projectId = GetProjectId(dbObject);

			GeneratedWorkList dublicatedWorkList = generatedWorkLists.FirstOrDefault(x => x.Project != dbObject && GetProjectId(x.Project) == projectId);
			if (dublicatedWorkList != null)
			{
				KeyValuePair<FullDocumentTemplate, List<GeneratedWork>>[] works = generatedWorkList.GeneratedWorks
					.Where(x => !dublicatedWorkList.GeneratedWorks.ContainsKey(x.Key))
					.ToArray();

				foreach (KeyValuePair<FullDocumentTemplate, List<GeneratedWork>> keyPairValue in works)
				{
					dublicatedWorkList.GeneratedWorks.Add(new KeyValuePair<FullDocumentTemplate, List<GeneratedWork>>(keyPairValue.Key, keyPairValue.Value));
				}
			}
		}

		public bool ContainsGeneratedWorkList(IDbObject packedProject, FullDocumentTemplate documentTemplate)
		{
			Project project = UnpackProject(packedProject);
			GeneratedWorkList generatedWorkList = generatedWorkLists.FirstOrDefault(x => project.Id == UnpackProject(x.Project).Id);
			return generatedWorkList != null && generatedWorkList.GeneratedWorks.ContainsKey(documentTemplate);
		}

		#endregion

		public int GetProjectId(IDbObject dbObject)
		{
			return dbObject is Project project ? project.Id : ((AlternativeProjectName)dbObject).ProjectId.Value;
		}

		public Project UnpackProject(IDbObject dbObject)
		{
			return dbObject as Project ?? ((AlternativeProjectName)dbObject).Project;
		}
	}
}
