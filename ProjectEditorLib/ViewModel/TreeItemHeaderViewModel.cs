using MaterialDesignThemes.Wpf;
using Mvvm;
using Mvvm.Commands;
using ProjectEditorLib.Model;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ProjectEditorLib.ViewModel
{
	public class TreeItemHeaderViewModel : DependencyObject, IEnumerable<TreeItemHeaderViewModel>
	{
		ProjectNode model = null;

		TreeItemHeaderViewModel parrent = null;
		ObservableRangeCollection<TreeItemHeaderViewModel> childs = new ObservableRangeCollection<TreeItemHeaderViewModel>();

		public TreeItemHeaderViewModel()
		{
			InitCommands();
		}

		#region Properties

		public ProjectNodeType NodeType
		{
			get { return (ProjectNodeType)GetValue(NodeTypeProperty); }
			set { SetValue(NodeTypeProperty, value); }
		}
		public static readonly DependencyProperty NodeTypeProperty = DependencyProperty.Register(nameof(NodeType), typeof(ProjectNodeType), typeof(TreeItemHeaderViewModel));

		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}
		public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(TreeItemHeaderViewModel));

		public IEnumerable<TreeItemHeaderViewModel> Childs => childs;

		public IEnumerator<TreeItemHeaderViewModel> GetEnumerator() => childs.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => childs.GetEnumerator();

		public ContextMenu ContextMenu
		{
			get { return (ContextMenu)GetValue(ContextMenuProperty); }
			set { SetValue(ContextMenuProperty, value); }
		}
		public static readonly DependencyProperty ContextMenuProperty = DependencyProperty.Register(nameof(ContextMenu), typeof(ContextMenu), typeof(TreeItemHeaderViewModel));

		#endregion

		#region Commands

		private void InitCommands()
		{
			removeCommand = new Command(OnRemoveExecute);
		}

		private Command CreateAddCommand(ProjectNodeType param)
		{
			return new Command(() => AddCommand?.Execute(param));
		}

		public Command<ProjectNodeType> AddCommand { get; set; }

		public Command RemoveCommand { get; set; }
		private Command removeCommand;
		private void OnRemoveExecute()
		{
			RemoveCommand?.Execute();
		}

		#endregion

		#region Methods

		private void ReInitContextMenu()
		{
			ContextMenu = new ContextMenu();
			List<Control> itemsSource = new List<Control>();
			switch (model.Type)
			{
				case ProjectNodeType.Project:
					itemsSource.Add(new MenuItem { Header = "Додати епізод", Command = CreateAddCommand(ProjectNodeType.Episode) });
					break;
				case ProjectNodeType.ProjectWithoutEpisodes:
					itemsSource.Add(new MenuItem { Header = "Додати бек", Command = CreateAddCommand(ProjectNodeType.Back) });
					itemsSource.Add(new MenuItem { Header = "Додати крафт", Command = CreateAddCommand(ProjectNodeType.Craft) });
					break;
				case ProjectNodeType.Episode:
					itemsSource.Add(new MenuItem { Header = "Додати бек", Command = CreateAddCommand(ProjectNodeType.Back) });
					itemsSource.Add(new MenuItem { Header = "Додати крафт", Command = CreateAddCommand(ProjectNodeType.Craft) });
					itemsSource.Add(new Separator());
					itemsSource.Add(new MenuItem { Header = "Видалити епізод", Command = removeCommand });
					break;
				case ProjectNodeType.Back:
					itemsSource.Add(new MenuItem { Header = "Додати мініігру", Command = CreateAddCommand(ProjectNodeType.Minigame) });
					itemsSource.Add(new MenuItem { Header = "Додати діалог", Command = CreateAddCommand(ProjectNodeType.Dialog) });
					itemsSource.Add(new MenuItem { Header = "Додати хог", Command = CreateAddCommand(ProjectNodeType.Hog) });
					itemsSource.Add(new Separator());
					itemsSource.Add(new MenuItem { Header = "Видалити бек", Command = removeCommand });
					break;
				case ProjectNodeType.Craft:
					itemsSource.Add(new MenuItem { Header = "Додати мініігру", Command = CreateAddCommand(ProjectNodeType.Minigame) });
					itemsSource.Add(new MenuItem { Header = "Додати хог", Command = CreateAddCommand(ProjectNodeType.Hog) });
					itemsSource.Add(new Separator());
					itemsSource.Add(new MenuItem { Header = "Видалити крафт", Command = removeCommand });
					break;
				case ProjectNodeType.Dialog:
					itemsSource.Add(new MenuItem { Header = "Видалити діалог", Command = removeCommand });
					break;
				case ProjectNodeType.Hog:
					itemsSource.Add(new MenuItem { Header = "Додати мініігру", Command = CreateAddCommand(ProjectNodeType.Minigame) });
					itemsSource.Add(new Separator());
					itemsSource.Add(new MenuItem { Header = "Видалити хог", Command = removeCommand });
					break;
				case ProjectNodeType.Minigame:
					itemsSource.Add(new MenuItem { Header = "Додати регіони", Command = CreateAddCommand(ProjectNodeType.Regions) });
					itemsSource.Add(new Separator());
					itemsSource.Add(new MenuItem { Header = "Видалити мінігру", Command = removeCommand });
					break;
				case ProjectNodeType.Regions:
					itemsSource.Add(new MenuItem { Header = "Видалити регіони", Command = removeCommand });
					break;
			}
			ContextMenu.ItemsSource = itemsSource;
		}

		public void SetModel(ProjectNode model)
		{
			this.model = model;
			NodeType = model.Type;
			Text = model.Text;

			ReInitContextMenu();
		}

		public void AddChild(TreeItemHeaderViewModel child)
		{
			childs.Add(child);
			child.parrent = this;
		}

		public void Remove()
		{
			parrent.childs.Remove(this);
		}

		#endregion

	}
}
