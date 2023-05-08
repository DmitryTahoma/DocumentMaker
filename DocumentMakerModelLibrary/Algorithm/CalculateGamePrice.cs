using Dml.Model.Back;
using Dml.Model.Template;
using DocumentMakerModelLibrary.Controls;
using System;
using System.Collections.Generic;

namespace DocumentMakerModelLibrary.Algorithm
{
	public static class CalculateGamePrice
	{
		public static void GamePrice(ref Dictionary<string, int> _games, IEnumerable<FullBackDataModel> _models, IList<GameObject> gameObjects, WorkType _workType)
		{
			_games.Clear();

			foreach (FullBackDataModel model in _models)
			{
				if ((_workType == WorkType.Development && !model.IsRework) || (_workType == WorkType.Rework && model.IsRework) || _workType == WorkType.All)
				{

					int price = -1;
					if (int.TryParse(model.SumText, out price))
					{
						if (model.Type == BackType.Other)
						{
							List<int> OtherGamesPrice = new List<int>();
							List<string> OtherGamesName = new List<string>();

							foreach (GameObject game in gameObjects)
							{
								FindGame(model.OtherText, game.Name, ref OtherGamesName);

								foreach (string altName in game.AltName)
								{
									FindGame(model.OtherText, altName, ref OtherGamesName);
								}
							}

							if (OtherGamesName.Count > 0)
							{
								int otherPrice = 0;
								foreach (string name in OtherGamesName)
								{
									OtherGamesPrice.Add(price / OtherGamesName.Count);
									otherPrice += price / OtherGamesName.Count;
								}

								OtherGamesPrice[0] += price - otherPrice;

								for (int i = 0; i < OtherGamesName.Count; i++)
								{
									AddGameSum(ref _games, OtherGamesName[i], OtherGamesPrice[i]);
								}
							}
							else
							{
								AddGameSum(ref _games, "Не визначено", price);
							}
						}
						else
						{
							bool findGame = false;
							foreach (GameObject game in gameObjects)
							{
								if (string.Compare(model.GameName, game.Name, StringComparison.OrdinalIgnoreCase) == 0)
								{
									findGame = true;
									AddGameSum(ref _games, model.GameName, price);
									break;
								}

								foreach (string altName in game.AltName)
								{
									if (string.Compare(model.GameName, altName, StringComparison.OrdinalIgnoreCase) == 0)
									{
										findGame = true;
										AddGameSum(ref _games, altName, price);
										break;
									}
								}

								if (findGame) break;
							}

							if (!findGame)
								AddGameSum(ref _games, "Не визначено", price);
						}
					}
				}
			}
		}

		private static bool FindGame(string _otherText, string _gameName, ref List<string> _otherGamesName)
		{
			bool result = false;
			string otherText = _otherText;
			string[] replaceSymbolQuotes = { "“", "”", "„", "‟", "«", "»", };

			foreach (string symbol in replaceSymbolQuotes)
				otherText = otherText.Replace(symbol, "\"");

			otherText = otherText.Replace(Convert.ToChar(160), ' ');

			bool findQuote = false;
			List<string> gameNameList = new List<string>();
			string tempName = string.Empty;
			foreach (char symbol in otherText)
			{
				if('"' == symbol)
				{
					if (findQuote)
					{
						findQuote = false;
						if(!string.IsNullOrEmpty(tempName))
							gameNameList.Add(tempName.Trim());

						tempName = string.Empty;
					}
					else findQuote = true;
				}
				else if(findQuote)
					tempName += symbol;
			}

			foreach (string nameGame in gameNameList)
			{
				if (string.Compare(nameGame, _gameName, StringComparison.OrdinalIgnoreCase) == 0)
				{
					result = true;
					break;
				}
			}

			if (result)
			{
				if (!_otherGamesName.Contains(_gameName))
					_otherGamesName.Add(_gameName);
			}

			return result;
		}

		private static void AddGameSum(ref Dictionary<string, int> _game, string _name, int _price)
		{
			if (_price >= 0)
			{
				if (!_game.ContainsKey(_name))
					_game.Add(_name, _price);
				else
					_game[_name] += _price;
			}
		}
	}
}
