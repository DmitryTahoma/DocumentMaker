using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace ProjectParser
{
	class Program
	{
		static string sourceFilename => "Source.xlsx";

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

			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("-------------------------------------------------");
			Console.ForegroundColor = ConsoleColor.White;

			data.RemoveAll(x => x.Value.Count <= 0);

			foreach(KeyValuePair<string, List<ParsedObj>> project in data)
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

				string str;
				do
				{
					Console.Write("1 Insert\n2 Skip\n> ");
					str = Console.ReadLine();

				} while (str != "1" && str != "2");

				if (str == "2") continue;

				for (int i = 0; i < project.Value.Count; ++i)
				{
					Console.Write('-');
				}

				Console.CursorLeft = 0;
				foreach(ParsedObj parsedObj1 in project.Value)
				{
					// insert to db

					Console.Write('+');
				}
				Console.WriteLine();
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
	}
}
