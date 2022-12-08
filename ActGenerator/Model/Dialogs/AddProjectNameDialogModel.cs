﻿using ProjectsDb;
using ProjectsDb.Context;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ActGenerator.Model.Dialogs
{
	class AddProjectNameDialogModel : ProjectsDbConnector
	{
		public Task<IEnumerable<Project>> LoadProjectsAsync()
		{
			return Task.Run(() =>
			{
				List<Project> projects = new List<Project>(db.Projects);
				foreach (Project project in projects)
				{
					project.AlternativeNames = db.AlternativeProjectNames.Where(x => x.ProjectId == project.Id).ToList();
				}
				return (IEnumerable<Project>)projects;
			});
		}
	}
}
