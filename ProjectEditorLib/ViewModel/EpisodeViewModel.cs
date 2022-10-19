using Db.Context;
using Db.Context.BackPart;
using System.Windows;

namespace ProjectEditorLib.ViewModel
{
	public class EpisodeViewModel : DependencyObject, IDbObjectViewModel
	{
		#region Properties

		public string EpisodeNumber
		{
			get { return (string)GetValue(EpisodeNumberProperty); }
			set { SetValue(EpisodeNumberProperty, value); }
		}
		public static readonly DependencyProperty EpisodeNumberProperty = DependencyProperty.Register(nameof(EpisodeNumber), typeof(string), typeof(EpisodeViewModel));

		public string EpisodeName
		{
			get { return (string)GetValue(EpisodeNameProperty); }
			set { SetValue(EpisodeNameProperty, value); }
		}
		public static readonly DependencyProperty EpisodeNameProperty = DependencyProperty.Register(nameof(EpisodeName), typeof(string), typeof(EpisodeViewModel));

		#endregion

		#region Methods

		public IDbObject UpdateContext(IDbObject dbObject)
		{
			Episode episode;

			if(dbObject == null)
			{
				episode = new Episode();
			}
			else
			{
				episode = dbObject as Episode;
			}

			if (episode != null)
			{
				episode.Name = EpisodeName;
				episode.Number = int.Parse(EpisodeNumber);
				dbObject = episode;
			}

			return dbObject;
		}

		public void SetFromContext(IDbObject dbObject)
		{
			if(dbObject is Episode episode)
			{
				EpisodeName = episode.Name;
				EpisodeNumber = episode.Number.ToString();
			}
			else
			{
				EpisodeName = string.Empty;
				EpisodeNumber = string.Empty;
			}
		}

		#endregion
	}
}
