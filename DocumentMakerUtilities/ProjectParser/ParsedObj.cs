using ProjectsDb.Context;
using System;
using System.Collections.Generic;

namespace ProjectParser
{
	class ParsedObj
	{
		public string BackName { get; set; } = null;
		public uint? BackRegions { get; set; } = null;
		public string HogName { get; set; } = null;
		public uint? HogRegions { get; set; } = null;
		public string Minigames { get; set; } = null;
		public string Dialogs { get; set; } = null;

		public void WriteToConsoleTable(int maxBackLength, int maxHogLength, int maxMinigamesLength)
		{
			Console.ForegroundColor = ConsoleColor.Blue;
			Console.Write(BackName.Replace('\n', ';') ?? "null");
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.CursorLeft = maxBackLength + 1;
			Console.Write(BackRegions?.ToString()?.Replace('\n', ';') ?? "null");
			Console.ForegroundColor = ConsoleColor.Blue;
			Console.CursorLeft = maxBackLength + 6;
			Console.Write(HogName?.Replace('\n', ';') ?? "null");
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.CursorLeft = maxBackLength + maxHogLength + 7;
			Console.Write(HogRegions?.ToString()?.Replace('\n', ';') ?? "null");
			Console.ForegroundColor = ConsoleColor.Blue;
			Console.CursorLeft = maxBackLength + maxHogLength + 12;
			Console.Write(Minigames?.Replace('\n', ';') ?? "null");
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.CursorLeft = maxBackLength + maxHogLength + maxMinigamesLength + 13;
			Console.WriteLine(Dialogs?.Replace('\n', ';') ?? "null");
			Console.ForegroundColor = ConsoleColor.White;
		}

		public Back ConvertToBack(Project project, BackType backType)
		{
			int lastDot = BackName.LastIndexOf('.');

			float backNumber = 0;
			if (lastDot > 0 && float.TryParse(BackName.Substring(0, lastDot).Trim().Replace('.', ','), out float num))
			{
				backNumber = num;
			}

			string backName = BackName;
			if (lastDot > 0) backName = backName.Substring(lastDot + 1, backName.Length - lastDot - 1);
			backName = backName.Trim();

			return new Back
			{
				BackType = backType,
				Name = backName,
				Number = backNumber,
				Project = project,

				Regions = new List<CountRegions> 
				{
					new CountRegions { Count = (int)(BackRegions ?? 0) }
				},
			};
		}

		public bool HaveHog()
		{
			return HogName != null;
		}

		public Back ConvertToHog(Project project, Back baseBack, BackType backType)
		{
			int lastDot = HogName.LastIndexOf('.');

			string hogName = HogName;
			if (lastDot > 0) hogName = hogName.Substring(lastDot + 1, hogName.Length - lastDot - 1);
			hogName = hogName.Trim();

			return new Back
			{
				BackType = backType,
				Name = hogName,
				BaseBack = baseBack,
				Number = 1,
				Project = project,

				Regions = new List<CountRegions>
				{
					new CountRegions { Count = (int)(HogRegions ?? 0) }
				},
			};
		}

		public bool HaveMinigames()
		{
			return Minigames != null;
		}

		public IEnumerable<Back> ConvertToMinigames(Project project, Back baseBack, BackType backType)
		{
			string[] minigames = Minigames.Split('\n');

			int curMgNum = 1;
			foreach(string minigame in minigames)
			{
				if (string.IsNullOrWhiteSpace(minigame)) continue;

				int lastDot = minigame.LastIndexOf('.');

				string mgName = minigame;
				if (lastDot > 0) mgName = mgName.Substring(lastDot + 1, mgName.Length - lastDot - 1);
				mgName = mgName.Trim();

				yield return new Back
				{
					BackType = backType,
					Name = mgName,
					BaseBack = baseBack,
					Number = curMgNum,
					Project = project,
				};

				++curMgNum;
			}
		}

		public bool HaveDialogs()
		{
			return Dialogs != null;
		}

		public IEnumerable<Back> ConvertToDialogs(Project project, Back baseBack, BackType backType)
		{
			string[] dialogs = Dialogs.Split('\n');

			int curDialogNum = 1;
			foreach(string dialog in dialogs)
			{
				if (string.IsNullOrWhiteSpace(dialog)) continue;

				int lastDot = dialog.LastIndexOf('.');

				string dialogName = dialog;
				if (lastDot > 0) dialogName = dialogName.Substring(lastDot + 1, dialogName.Length - lastDot - 1);
				dialogName = dialogName.Trim();

				yield return new Back
				{
					BackType = backType,
					Name = dialogName,
					BaseBack = baseBack,
					Number = curDialogNum,
					Project = project,
				};

				++curDialogNum;
			}
		}
	}
}
