using Dml;
using DocumentMaker.Security;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Xml.Serialization;

namespace ActGenerator.Model
{
	public class ActGeneratorSession
	{
		public static string SaveFileName => "session_act_generator.xml";
		public static string CurrentSaveFileName => Path.Combine(PathHelper.ExecutingAssemblyPath, SaveFileName);

		public double WindowTop { get; set; } = -1;
		public double WindowLeft { get; set; } = -1;
		public double WindowHeight { get; set; } = 500;
		public double WindowWidth { get; set; } = 900;
		public WindowState WindowState { get; set; } = WindowState.Maximized;

		public string MinSumText { get; set; } = "1200";
		public string MaxSumText { get; set; } = "2500";
		public DateTime? MinActDate { get; set; } = null;
		public DateTime? MaxActDate { get; set; } = null;
		public bool CollapseRegionsWorks { get; set; } = true;
		public DateTimeItem SelectedDateTimeItem { get; set; } = null;
		public CryptedConnectionString CryptedConnectionString { get; set; }
		public string LastOpenedProjectName { get; set; } = null;

		[XmlIgnore]
		public bool ApplicationLoaded { get; private set; } = false;

		public void Load()
		{
			if (File.Exists(CurrentSaveFileName))
			{
				XmlSerializer loader = new XmlSerializer(typeof(ActGeneratorSession));
				using (FileStream fstream = new FileStream(CurrentSaveFileName, FileMode.Open))
				{
					if (loader.Deserialize(fstream) is ActGeneratorSession loadedObj)
					{
						WindowTop = loadedObj.WindowTop;
						WindowLeft = loadedObj.WindowLeft;
						WindowHeight = loadedObj.WindowHeight;
						WindowWidth = loadedObj.WindowWidth;
						WindowState = loadedObj.WindowState;

						MinSumText = loadedObj.MinSumText;
						MaxSumText = loadedObj.MaxSumText;
						MinActDate = loadedObj.MinActDate;
						MaxActDate = loadedObj.MaxActDate;
						CollapseRegionsWorks = loadedObj.CollapseRegionsWorks;
						SelectedDateTimeItem = loadedObj.SelectedDateTimeItem;
						CryptedConnectionString = loadedObj.CryptedConnectionString;
						LastOpenedProjectName = loadedObj.LastOpenedProjectName;
					}
				}

				if (CryptedConnectionString != null && !CryptedConnectionString.IsCrypted)
				{
					CryptedConnectionString.Crypt();
					Save();
				}

				ApplicationLoaded = true;
			}
		}

		public void Save()
		{
			XmlSerializer saver = new XmlSerializer(typeof(ActGeneratorSession));
			using (FileStream fstream = new FileStream(CurrentSaveFileName, FileMode.Create))
			{
				saver.Serialize(fstream, this);
			}
		}

		public void ReopenApplicationElevatedRights()
		{
			ProcessStartInfo psi = new ProcessStartInfo
			{
				FileName = Assembly.GetExecutingAssembly().Location,
				Verb = "runas",
			};
			try
			{
				Process.Start(psi);
			}
			finally
			{
				Application.Current.Shutdown(0);
			}
		}
	}
}
