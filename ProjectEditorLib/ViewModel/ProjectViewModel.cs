using DocumentMakerThemes.ViewModel;
using MaterialDesignThemes.Wpf;
using Mvvm.Commands;
using ProjectEditorLib.Model;
using ProjectEditorLib.View;
using ProjectsDb.Context;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ProjectEditorLib.ViewModel
{
	public class ProjectViewModel : BaseDbObjectViewModel, IDbObjectViewModel, ISnackbarRequired
	{
		UIElementCollection projectNamesCollection = null;
		private bool haveChildChanges;

		Snackbar snackbar = null;

		public ProjectViewModel() : base() { }

		#region Properties

		public string ProjectName
		{
			get { return (string)GetValue(ProjectNameProperty); }
			set { SetValue(ProjectNameProperty, value); }
		}
		public static readonly DependencyProperty ProjectNameProperty = DependencyProperty.Register(nameof(ProjectName), typeof(string), typeof(ProjectViewModel));

		#endregion

		#region Commands

		protected override void InitCommands()
		{
			base.InitCommands();

			BindAltProjectNamesCollection = new Command<UIElementCollection>(OnBindAltProjectNamesCollectionExecute);
			AddAltProjectName = new Command(OnAddAltProjectNameExecute);
			DeleteAltProjectName = new Command<AlternativeProjectNameView>(OnDeleteAltProjectNameExecute);
		}

		public Command<UIElementCollection> BindAltProjectNamesCollection { get; private set; }
		private void OnBindAltProjectNamesCollectionExecute(UIElementCollection collection)
		{
			projectNamesCollection = collection;
		}

		public Command AddAltProjectName { get; private set; }
		private void OnAddAltProjectNameExecute()
		{
			AddAlternativeProjectName();
			HaveUnsavedChanges = true;
		}

		public Command<AlternativeProjectNameView> DeleteAltProjectName { get; private set; }
		private void OnDeleteAltProjectNameExecute(AlternativeProjectNameView sender)
		{
			projectNamesCollection.Remove(sender);
			HaveUnsavedChanges = true;
		}

		public bool HaveChildChanges
		{
			get => haveChildChanges;
			set 
			{
				haveChildChanges = value; 

				if(haveChildChanges)
				{
					HaveUnsavedChanges = true;
				}
			}
		}

		#endregion

		#region Methods

		public override void SetFromContext(IDbObject dbObject)
		{
			if (dbObject is Project project)
			{
				ProjectName = project.Name;

				projectNamesCollection.Clear();
				if (project.AlternativeNames != null)
				{
					while(projectNamesCollection.Count > project.AlternativeNames.Count)
					{
						projectNamesCollection.RemoveAt(0);
					}

					while(projectNamesCollection.Count < project.AlternativeNames.Count)
					{
						AddAlternativeProjectName();
					}

					IEnumerator<AlternativeProjectName> projectAlternativeNamesEnum = project.AlternativeNames.GetEnumerator();
					IEnumerator projectNamesCollectionEnum = projectNamesCollection.GetEnumerator();
					while(projectAlternativeNamesEnum.MoveNext() && projectNamesCollectionEnum.MoveNext())
					{
						AlternativeProjectNameView altProjectName = (AlternativeProjectNameView)projectNamesCollectionEnum.Current;
						AlternativeProjectNameViewModel altProjectNameViewModel = (AlternativeProjectNameViewModel)altProjectName.DataContext;
						altProjectNameViewModel.SetFromContext(projectAlternativeNamesEnum.Current);
					}
				}
				context = project;
			}
			else
			{
				ProjectName = string.Empty;
			}
		}

		public override IDbObject UpdateContext(IDbObject dbObject)
		{
			Project project;

			if (dbObject == null)
			{
				project = new Project();
			}
			else
			{
				project = dbObject as Project;
			}

			if (project != null)
			{
				project.Name = ProjectName;
				List<AlternativeProjectName> currentAlternativeProjectNames = new List<AlternativeProjectName>();
				foreach(AlternativeProjectNameView elem in projectNamesCollection)
				{
					AlternativeProjectNameViewModel viewModel = (AlternativeProjectNameViewModel)elem.DataContext;
					currentAlternativeProjectNames.Add(new AlternativeProjectName { Id = viewModel.GetModel(), Name = viewModel.AltProjectName, ProjectId = project.Id });
				}
				project.AlternativeNames = currentAlternativeProjectNames;

				dbObject = project;
				context = project;
			}

			return dbObject;
		}

		private void AddAlternativeProjectName(AlternativeProjectName context = null)
		{
			AlternativeProjectNameView altProjectName = new AlternativeProjectNameView();
			AlternativeProjectNameViewModel altProjectNameViewModel = (AlternativeProjectNameViewModel)altProjectName.DataContext;
			altProjectNameViewModel.DeleteAltProjectName = DeleteAltProjectName;
			if (context != null)
			{
				altProjectNameViewModel.SetFromContext(context);
			}
			projectNamesCollection.Add(altProjectName);

			Binding haveUnsavedChangesBinding = new Binding(nameof(HaveChildChanges))
			{
				Source = this,
				Path = new PropertyPath(nameof(HaveChildChanges)),
				Mode = BindingMode.TwoWay,
				UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
			};
			BindingOperations.SetBinding(altProjectNameViewModel, HaveUnsavedChangesProperty, haveUnsavedChangesBinding);
		}

		public void SetSnackbar(Snackbar snackbar)
		{
			this.snackbar = snackbar;
		}

		#endregion
	}
}
