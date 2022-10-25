using Dml;
using Dml.Model.Session;
using System.IO;
using System.Windows;

namespace ActGenerator.Model
{
	class ActGeneratorSession
	{
		public static string SaveFileName => "session_act_generator.xml";
		public static string CurrentSaveFileName => Path.Combine(PathHelper.ExecutingAssemblyPath, SaveFileName);

		public double WindowTop { get; set; } = -1;
		public double WindowLeft { get; set; } = -1;
		public double WindowHeight { get; set; } = 500;
		public double WindowWidth { get; set; } = 900;
		public WindowState WindowState { get; set; } = WindowState.Normal;

		public void Load()
		{
			XmlLoader loader = new XmlLoader();
			if(loader.TryLoad(CurrentSaveFileName))
			{
				loader.SetLoadedProperties(this);
			}
		}

		public void Save()
		{
			XmlSaver saver = new XmlSaver();
			saver.AppendAllProperties(this);
			saver.Save(Path.Combine(CurrentSaveFileName));
		}
	}
}
