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
			Project project = dbObject as Project ?? ((AlternativeProjectName)dbObject).Project;

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
	}
}
