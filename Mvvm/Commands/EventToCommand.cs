using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Mvvm.Commands
{
	public class EventToCommand : TriggerAction<DependencyObject>
	{
		public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object), typeof(EventToCommand), new PropertyMetadata(null, delegate (DependencyObject s, DependencyPropertyChangedEventArgs e)
		{
			EventToCommand eventToCommand = s as EventToCommand;
			if (eventToCommand == null)
			{
				return;
			}
			if (eventToCommand.AssociatedObject == null)
			{
				return;
			}
			eventToCommand.EnableDisableElement();
		}));

		public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(EventToCommand), new PropertyMetadata(null, delegate (DependencyObject s, DependencyPropertyChangedEventArgs e)
		{
			EventToCommand.OnCommandChanged(s as EventToCommand, e);
		}));

		public static readonly DependencyProperty MustToggleIsEnabledProperty = DependencyProperty.Register("MustToggleIsEnabled", typeof(bool), typeof(EventToCommand), new PropertyMetadata(false, delegate (DependencyObject s, DependencyPropertyChangedEventArgs e)
		{
			EventToCommand eventToCommand = s as EventToCommand;
			if (eventToCommand == null)
			{
				return;
			}
			if (eventToCommand.AssociatedObject == null)
			{
				return;
			}
			eventToCommand.EnableDisableElement();
		}));
		private object _commandParameterValue;
		private bool? _mustToggleValue;

		public ICommand Command
		{
			get
			{
				return (ICommand)base.GetValue(EventToCommand.CommandProperty);
			}
			set
			{
				base.SetValue(EventToCommand.CommandProperty, value);
			}
		}

		public object CommandParameter
		{
			get
			{
				return base.GetValue(EventToCommand.CommandParameterProperty);
			}
			set
			{
				base.SetValue(EventToCommand.CommandParameterProperty, value);
			}
		}

		public object CommandParameterValue
		{
			get
			{
				return this._commandParameterValue ?? this.CommandParameter;
			}
			set
			{
				this._commandParameterValue = value;
				this.EnableDisableElement();
			}
		}

		public bool MustToggleIsEnabled
		{
			get
			{
				return (bool)base.GetValue(EventToCommand.MustToggleIsEnabledProperty);
			}
			set
			{
				base.SetValue(EventToCommand.MustToggleIsEnabledProperty, value);
			}
		}

		public bool MustToggleIsEnabledValue
		{
			get
			{
				if (this._mustToggleValue.HasValue)
				{
					return this._mustToggleValue.Value;
				}
				return this.MustToggleIsEnabled;
			}
			set
			{
				this._mustToggleValue = new bool?(value);
				this.EnableDisableElement();
			}
		}

		public bool PassEventArgsToCommand
		{
			get;
			set;
		}

		protected override void OnAttached()
		{
			base.OnAttached();
			this.EnableDisableElement();
		}
		private Control GetAssociatedObject()
		{
			return base.AssociatedObject as Control;
		}

		private ICommand GetCommand()
		{
			return this.Command;
		}

		public void Invoke()
		{
			this.Invoke(null);
		}

		protected override void Invoke(object parameter)
		{
			if (this.AssociatedElementIsDisabled())
			{
				return;
			}
			ICommand command = this.GetCommand();
			object obj = this.CommandParameterValue;
			if (obj == null && this.PassEventArgsToCommand)
			{
				obj = parameter;
			}
			if (command != null && command.CanExecute(obj))
			{
				command.Execute(obj);
			}
		}

		private static void OnCommandChanged(EventToCommand element, DependencyPropertyChangedEventArgs e)
		{
			if (element == null)
			{
				return;
			}
			if (e.OldValue != null)
			{
				((ICommand)e.OldValue).CanExecuteChanged -= new EventHandler(element.OnCommandCanExecuteChanged);
			}
			ICommand command = (ICommand)e.NewValue;
			if (command != null)
			{
				command.CanExecuteChanged += new EventHandler(element.OnCommandCanExecuteChanged);
			}
			element.EnableDisableElement();
		}

		private bool AssociatedElementIsDisabled()
		{
			Control associatedObject = this.GetAssociatedObject();
			return base.AssociatedObject == null || (associatedObject != null && !associatedObject.IsEnabled);
		}

		private void EnableDisableElement()
		{
			Control associatedObject = this.GetAssociatedObject();
			if (associatedObject == null)
			{
				return;
			}
			ICommand command = this.GetCommand();
			if (this.MustToggleIsEnabledValue && command != null)
			{
				associatedObject.IsEnabled = command.CanExecute(this.CommandParameterValue);
			}
		}

		private void OnCommandCanExecuteChanged(object sender, EventArgs e)
		{
			this.EnableDisableElement();
		}
	}
}
