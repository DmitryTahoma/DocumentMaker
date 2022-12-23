using DocumentMakerModelLibrary;
using DocumentMakerModelLibrary.OfficeFiles.Human;
using System.Collections.Generic;
using System.Windows;

namespace ActGenerator.ViewModel.Controls
{
	public class HumenListItemControlViewModel : DependencyObject
	{
		private HumanData model = null;

		public HumenListItemControlViewModel()
		{
			SelectedTemplates = new List<object>();
		}

		#region Properties

		public string Name
		{
			get { return (string)GetValue(NameProperty); }
			set { SetValue(NameProperty, value); }
		}
		public static readonly DependencyProperty NameProperty = DependencyProperty.Register(nameof(Name), typeof(string), typeof(HumenListItemControlViewModel));

		public bool IsChecked
		{
			get { return (bool)GetValue(IsCheckedProperty); }
			set { SetValue(IsCheckedProperty, value); }
		}
		public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(nameof(IsChecked), typeof(bool), typeof(HumenListItemControlViewModel));

		public string SumText
		{
			get { return (string)GetValue(SumTextProperty); }
			set { SetValue(SumTextProperty, value); }
		}
		public static readonly DependencyProperty SumTextProperty = DependencyProperty.Register(nameof(SumText), typeof(string), typeof(HumenListItemControlViewModel));

		public IEnumerable<FullDocumentTemplate> DocumentTemplatesList
		{
			get { return (IEnumerable<FullDocumentTemplate>)GetValue(DocumentTemplatesListProperty); }
			set { SetValue(DocumentTemplatesListProperty, value); }
		}
		public static readonly DependencyProperty DocumentTemplatesListProperty = DependencyProperty.Register(nameof(DocumentTemplatesList), typeof(IEnumerable<FullDocumentTemplate>), typeof(HumenListItemControlViewModel));

		public List<object> SelectedTemplates
		{
			get { return (List<object>)GetValue(SelectedTemplatesProperty); }
			set { SetValue(SelectedTemplatesProperty, value); }
		}
		public static readonly DependencyProperty SelectedTemplatesProperty = DependencyProperty.Register(nameof(SelectedTemplates), typeof(List<object>), typeof(HumenListItemControlViewModel));

		public HumanData Model
		{
			get => model;
			set
			{
				model = value;
				Name = model.Name;
			}
		}

		#endregion
	}
}
