using DocumentMakerModelLibrary.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DocumentMakerModelLibrary.Controls;
using Dml.Model.Template;

namespace DcmkComparer
{
	class Program
	{
		static string ProgramStartDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		static List<DcmkFile> Files = new List<DcmkFile>();
		static void Main(string[] args)
		{
			Console.OutputEncoding = System.Text.Encoding.Unicode;
			Console.InputEncoding = System.Text.Encoding.Unicode;
			LoadDcmk();
			int countEqual = 0;
			for (int i = 0; i < Files.Count; i++)
			{
				for (int j = i + 1; j < Files.Count; j++)
				{
					if(Files[i].TemplateType == Files[j].TemplateType)
					{
						bool IsEqual = Comparer(Files[i].BackDataModels, Files[j].BackDataModels, Files[i].TemplateType);

						if(IsEqual)
						{
							Console.ForegroundColor = ConsoleColor.White;
							countEqual++;
							if (DateTime.TryParse(Files[i].ActDateText, out DateTime d1) && DateTime.TryParse(Files[j].ActDateText, out DateTime d2))
							{
								TimeSpan time = d1 - d2;
								double days = time.TotalDays;
								Console.WriteLine("Файл 1 - " + Files[i].Name);
								Console.WriteLine("Файл 2 - " + Files[j].Name);
								Console.ForegroundColor = ConsoleColor.Green;
								Console.WriteLine("Разница в днях:" + days);
								Console.WriteLine();
								Console.WriteLine();		
							}
							else
							{
								Console.WriteLine("Файл 1 - " + Files[i].Name);
								Console.WriteLine("Файл 2 - " + Files[j].Name);
								Console.ForegroundColor = ConsoleColor.Green;
								Console.WriteLine("Разница в днях неизвестна");
								Console.WriteLine();
								Console.WriteLine();
							}

							break;
						}
					}
				}
			}

			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("Количество одинаковых файлов: " + countEqual);
			Console.ForegroundColor = ConsoleColor.White;
			Console.ReadKey();
		}

		static void LoadDcmk()
		{
			string fileMask = "*.dcmk";

			foreach (string fileName in Directory.GetFiles(ProgramStartDirectory, fileMask))
			{
				Files.Add(new DcmkFile(fileName));
			}

			foreach (DcmkFile file in Files)
			{
				file.Load();
			}
		}

		static bool Comparer(IList<FullBackDataModel> File1, IList<FullBackDataModel> File2, DocumentTemplateType TemplateType)
		{
			foreach (FullBackDataModel work in File1)
			{
				if (work.IsOtherType)
					continue;

				foreach (FullBackDataModel work2 in File2)
				{
					if (work.WorkObjectId == work2.WorkObjectId && work.EqualsWithoutWork(work2, TemplateType))
					{
						return true;
					}
				}
			}

			return false;
		}
	}
}
