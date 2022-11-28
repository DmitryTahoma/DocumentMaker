using DocumentMaker.Security;
using ProjectsDb;
using ProjectsDb.Context;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Serialization;

namespace ProjectParser
{
	partial class Program
	{
		static string sourceFilename => "Source.xlsx";
		const int countFill = -1;
		const bool enableTestFilling = countFill > 1;
		const string connectionStringFilename = "connection_string.xml";
		static CryptedConnectionString cryptedConnectionString = null;

		static void Main(string[] args)
		{
			if (!File.Exists(sourceFilename)) return;
			Maximize();
			Console.OutputEncoding = System.Text.Encoding.Unicode;
			Console.InputEncoding = System.Text.Encoding.Unicode;

			Parser parser = new Parser();
			List<KeyValuePair<string, List<ParsedObj>>> data = parser.ParseFromFile(sourceFilename);

			Console.ForegroundColor = ConsoleColor.DarkGreen;
			Console.WriteLine("End!");

			Console.ReadLine();

			if (enableTestFilling)
			{
				string title = Console.Title;

				for (int i = 0; i < countFill; ++i)
				{
					Console.Title = (i + 1).ToString() + " of " + countFill.ToString() + " filling | " + title;
					FillToDatabase(data);
				}
			}
			else
			{
				FillToDatabase(data);
			}

			Console.ReadLine();
		}

		[DllImport("user32.dll")]
		public static extern bool ShowWindow(IntPtr hWnd, int cmdShow);

		private static void Maximize()
		{
			Process p = Process.GetCurrentProcess();
			ShowWindow(p.MainWindowHandle, 3); //SW_MAXIMIZE = 3
		}

		private static string FillingMenu()
		{
			string str;
			do
			{
				Console.Write("1 Insert\n2 Skip\n> ");
				str = Console.ReadLine();

			} while (str != "1" && str != "2");
			return str;
		}

		private static void ProgressBarStart(int count)
		{
			for (int i = 0; i < count; ++i)
			{
				Console.Write('-');
			}
			Console.CursorLeft = 0;
		}

		private static void AddProgress()
		{
			Console.Write('+');
		}

		private static void FillToDatabase(List<KeyValuePair<string, List<ParsedObj>>> data)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("-------------------------------------------------");
			Console.ForegroundColor = ConsoleColor.White;

			data.RemoveAll(x => x.Value.Count <= 0);

			foreach (KeyValuePair<string, List<ParsedObj>> project in data)
			{
				int maxBackLength = project.Value.Max(x => x.BackName?.Length ?? 4);
				int maxHogLength = project.Value.Max(x => x.HogName?.Length ?? 4);
				int maxMinigamesLength = project.Value.Max(x => x.Minigames?.Length ?? 4);

				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine(project.Key);
				Console.ForegroundColor = ConsoleColor.White;
				foreach (ParsedObj parsedObj in project.Value)
				{
					parsedObj.WriteToConsoleTable(maxBackLength, maxHogLength, maxMinigamesLength);
				}

				if(!enableTestFilling)
				{
					string str = FillingMenu();
					if (str == "2") continue;
				}

				ProgressBarStart(project.Value.Count);

				foreach (ParsedObj parsedObj in project.Value)
				{
					FillObjToDatabase(parsedObj, project.Key);
					AddProgress();
				}
				Console.WriteLine();
			}
		}

		private static BackType GetBackType(ProjectsDbContext db, ProjectNodeType projectNodeType)
		{
			BackType dbBackType = db.BackTypes.FirstOrDefault(x => x.Name == projectNodeType.ToString());
			if (dbBackType == null)
			{
				dbBackType = db.BackTypes.Add(new BackType { Name = projectNodeType.ToString() });
			}
			return dbBackType;
		}

		private static void FillObjToDatabase(ParsedObj parsedObj, string projectName)
		{
			using (ProjectsDbContext db = new ProjectsDbContext(GetConnectionString()))
			{
				Project dbProject = db.Projects.FirstOrDefault(x => x.Name == projectName);
				if (dbProject == null)
				{
					dbProject = db.Projects.Add(new Project { Name = projectName });
				}

				BackType dbBackType = GetBackType(db, ProjectNodeType.Back);
				Back dbBack = db.Backs.Add(parsedObj.ConvertToBack(dbProject, dbBackType));

				if (parsedObj.HaveHog())
				{
					BackType dbHogType = GetBackType(db, ProjectNodeType.Hog);
					db.Backs.Add(parsedObj.ConvertToHog(dbProject, dbBack, dbHogType));
				}

				if(parsedObj.HaveMinigames())
				{
					BackType dbMgType = GetBackType(db, ProjectNodeType.Minigame);
					List<Back> minigames = parsedObj.ConvertToMinigames(dbProject, dbBack, dbMgType).ToList();
					if(minigames.Count > 0)
					{
						db.Backs.AddRange(minigames);
					}
				}

				if(parsedObj.HaveDialogs())
				{
					BackType dbDialogType = GetBackType(db, ProjectNodeType.Dialog);
					List<Back> dialogs = parsedObj.ConvertToDialogs(dbProject, dbBack, dbDialogType).ToList();
					if(dialogs.Count > 0)
					{
						db.Backs.AddRange(dialogs);
					}
				}

				db.SaveChanges();
			}
		}

		private static string GetConnectionString()
		{
			if (cryptedConnectionString == null)
			{
				XmlSerializer serializer = new XmlSerializer(typeof(CryptedConnectionString));
				using (FileStream fstream = new FileStream(connectionStringFilename, FileMode.Open))
				{
					cryptedConnectionString = serializer.Deserialize(fstream) as CryptedConnectionString;
				}

				if (cryptedConnectionString != null && !cryptedConnectionString.IsCrypted)
				{
					cryptedConnectionString.Crypt();
					using(FileStream fstream = new FileStream(connectionStringFilename, FileMode.Create))
					{
						serializer.Serialize(fstream, cryptedConnectionString);
					}
				}
			}
			return cryptedConnectionString.GetDecryptedConnectionString();
		}
	}
}
