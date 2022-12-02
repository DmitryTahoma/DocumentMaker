using ProjectsDb.Context;
using Mvvm;
using Mvvm.Commands;
using ProjectEditorLib.Model;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Dml;

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

		public bool ShowFullName { get; set; } = false;

		#endregion

		#region Commands

		private void InitCommands()
		{
			removeCommand = new Command(OnRemoveExecute);
			restoreCommand = new Command(OnRestoreExecute);
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

		public Command RestoreCommand { get; set; }
		private Command restoreCommand;
		private void OnRestoreExecute()
		{
			RestoreCommand?.Execute();
		}

		#endregion

		#region Methods

		public void InitProjectEditionContextMenu()
		{
			List<Control> itemsSource = new List<Control>();
			switch (model.Type)
			{
				case ProjectNodeType.Project:
					itemsSource.Add(new MenuItem { Header = "Додати епізод", Command = CreateAddCommand(ProjectNodeType.Episode), DataContext = NodeContextMenuItemType.Adding });
					itemsSource.Add(new MenuItem { Header = "Додати бек", Command = CreateAddCommand(ProjectNodeType.Back), DataContext = NodeContextMenuItemType.Adding });
					itemsSource.Add(new MenuItem { Header = "Додати", Command = CreateAddCommand(ProjectNodeType.Craft), DataContext = NodeContextMenuItemType.Adding });
					break;
				case ProjectNodeType.Episode:
					itemsSource.Add(new MenuItem { Header = "Додати бек", Command = CreateAddCommand(ProjectNodeType.Back), DataContext = NodeContextMenuItemType.Adding });
					itemsSource.Add(new MenuItem { Header = "Додати крафт", Command = CreateAddCommand(ProjectNodeType.Craft), DataContext = NodeContextMenuItemType.Adding });
					itemsSource.Add(new Separator());
					itemsSource.Add(new MenuItem { Header = "Видалити", Command = removeCommand, DataContext = NodeContextMenuItemType.Removing });
					break;
				case ProjectNodeType.Back:
					itemsSource.Add(new MenuItem { Header = "Додати мініігру", Command = CreateAddCommand(ProjectNodeType.Minigame), DataContext = NodeContextMenuItemType.Adding });
					itemsSource.Add(new MenuItem { Header = "Додати діалог", Command = CreateAddCommand(ProjectNodeType.Dialog), DataContext = NodeContextMenuItemType.Adding });
					itemsSource.Add(new MenuItem { Header = "Додати хог", Command = CreateAddCommand(ProjectNodeType.Hog), DataContext = NodeContextMenuItemType.Adding });
					itemsSource.Add(new Separator());
					itemsSource.Add(new MenuItem { Header = "Видалити", Command = removeCommand, DataContext = NodeContextMenuItemType.Removing });
					break;
				case ProjectNodeType.Craft:
					itemsSource.Add(new MenuItem { Header = "Додати мініігру", Command = CreateAddCommand(ProjectNodeType.Minigame), DataContext = NodeContextMenuItemType.Adding });
					itemsSource.Add(new MenuItem { Header = "Додати хог", Command = CreateAddCommand(ProjectNodeType.Hog), DataContext = NodeContextMenuItemType.Adding });
					itemsSource.Add(new Separator());
					itemsSource.Add(new MenuItem { Header = "Видалити", Command = removeCommand, DataContext = NodeContextMenuItemType.Removing });
					break;
				case ProjectNodeType.Dialog:
					itemsSource.Add(new MenuItem { Header = "Видалити", Command = removeCommand, DataContext = NodeContextMenuItemType.Removing });
					break;
				case ProjectNodeType.Hog:
					itemsSource.Add(new MenuItem { Header = "Додати мініігру", Command = CreateAddCommand(ProjectNodeType.Minigame), DataContext = NodeContextMenuItemType.Adding });
					itemsSource.Add(new Separator());
					itemsSource.Add(new MenuItem { Header = "Видалити", Command = removeCommand, DataContext = NodeContextMenuItemType.Removing });
					break;
				case ProjectNodeType.Minigame:
					itemsSource.Add(new MenuItem { Header = "Додати регіони", Command = CreateAddCommand(ProjectNodeType.Regions), DataContext = NodeContextMenuItemType.Adding });
					itemsSource.Add(new Separator());
					itemsSource.Add(new MenuItem { Header = "Видалити", Command = removeCommand, DataContext = NodeContextMenuItemType.Removing });
					break;
				case ProjectNodeType.Regions:
					itemsSource.Add(new MenuItem { Header = "Видалити", Command = removeCommand, DataContext = NodeContextMenuItemType.Removing });
					break;
			}
			ContextMenu = new ContextMenu { ItemsSource = itemsSource };
		}

		public void InitProjectRestoreContextMenu()
		{
			List<Control> itemsSource = new List<Control>();
			switch (model.Type)
			{
				case ProjectNodeType.Project:
					itemsSource.Add(new MenuItem { Header = "Відновити всі", Command = restoreCommand, DataContext = NodeContextMenuItemType.Restoring });
					itemsSource.Add(new Separator());
					itemsSource.Add(new MenuItem { Header = "Видалити всі назавжди", Command = removeCommand, DataContext = NodeContextMenuItemType.Removing });
					break;
				default:
					itemsSource.Add(new MenuItem { Header = "Відновити", Command = restoreCommand, DataContext = NodeContextMenuItemType.Restoring });
					itemsSource.Add(new Separator());
					itemsSource.Add(new MenuItem { Header = "Видалити назавжди", Command = removeCommand, DataContext = NodeContextMenuItemType.Removing });
					break;
			}
			ContextMenu = new ContextMenu { ItemsSource = itemsSource };
		}

		public void SetModel(ProjectNode model)
		{
			this.model = model;
			NodeType = model.Type;
			UpdateText();
		}

		public ProjectNode GetModel()
		{
			return model;
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

		public void UpdateText()
		{
			if (model.Context == null)
			{
				Text = "?";
			}
			else if(!ShowFullName)
			{
				Text = model.ToString();
			}
			else
			{
				string res = null;
				IDbObject context = model.Context;
				while(context != null)
				{
					IDbObject next = null;

					ProjectNodeType projectNodeType = ProjectNodeType.Back;
					if(context is CountRegions countRegions)
					{
						projectNodeType = ProjectNodeType.Regions;
						next = countRegions.Back;
					}
					else if(context is Back back)
					{
						if(back.BackType.Name == nameof(ProjectNodeType.Craft))
						{
							projectNodeType = ProjectNodeType.Craft;
						}
						next = back.BaseBack;
					}

					string str = model.ConextToString(projectNodeType, context);
					res = res != null
						? res.Insert(0, str + " ➔ ")
						: str;

					context = next;
				}

				Text = res;
			}
		}

		public TreeItemHeaderViewModel GetParrent()
		{
			return parrent;
		}

		#endregion

	}
}
