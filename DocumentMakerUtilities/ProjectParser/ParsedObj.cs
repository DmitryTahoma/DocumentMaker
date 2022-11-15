using System;

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
	}
}
