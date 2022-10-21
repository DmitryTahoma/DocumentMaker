using System;
using System.Collections.Generic;

namespace DocumentMaker.Model.Algorithm
{
	public static class MixingSumAlgorithm
	{
		/// <summary>
		/// Перекидывает суммы розробкы чтобы они были БОЛЬШЕ чем "_maxNumber"
		/// </summary>
		/// <param name="_minNumber">Сумма больше которой нужно сделать пункты</param>
		/// <param name="_isTossOutPidtrimka">Чтобы в конце сортировки, если не хватило то бралось из пидтрымки</param>
		/// <param name="_isRemoveIdenticalNumbers">Чтобы суммы пунктов не повторялись</param>
		/// <param name="_isRemoveLinePidtrimka">Если пункт меньше чем "minNumber" то чтобы сумма перекидывал в максимальную, а пункт удалялся</param>
		public static void MoreNumber(ref List<int> _rozrobka, ref List<int> _pidtrimka, int _minNumber, bool _isTossOutPidtrimka, bool _isRemoveIdenticalNumbers, bool _isRemoveLinePidtrimka)
		{
			//ПЕРЕКИДЫВАЕТ суммы розробкы чтобы они были БОЛЬШЕ чем _minNumber
			Random rnd = new Random();
			int percent = 10; // Какой процент от суммы перекидывать
			int minNumber = 400;
			float randomPrecent; // Смещение процента от percent
			int minPercent = -3; // Погрешность от percent
			int maxPercent = 5; // Погрешность от percent

			if (percent + minPercent < 0)
				minPercent = -percent + 1;

			int maxIndexNumberPidtrimka = -1;
			FindMaxIndex(_pidtrimka, ref maxIndexNumberPidtrimka); // Индекс максимальной суммы пидтрымкы

			int maxIndexNumberRozrobka = -1;
			FindMaxIndex(_rozrobka, ref maxIndexNumberRozrobka); // Индекс максимальной суммы розробкы
			int minIndexNumberRozrobka = -1;
			FindMinIndex(_rozrobka, ref minIndexNumberRozrobka); // Индекс минимальной суммы розробкы

			int sumFlip = 300; // Сколько будем перекидывать на другой пукнт

			bool isFlip = true;

			// Ограничение перебросов чтобы не зациклилось
			int totalSum = 0;
			foreach (int sum in _rozrobka)
			{
				totalSum += sum;
			}

			foreach (int sum in _pidtrimka)
			{
				totalSum += sum;
			}

			int counterFlip = (int)(totalSum * 1.5f) / (sumFlip * (percent + minPercent) / 100);

			while (isFlip)
			{
				isFlip = false;

				if (minIndexNumberRozrobka != -1)
				{
					int minSum = _rozrobka[minIndexNumberRozrobka];

					if (minSum >= _minNumber)
						break;

					randomPrecent = percent + ((float)rnd.NextDouble() * (maxPercent + (minPercent * -1)) + minPercent);

					if (maxIndexNumberRozrobka != -1 && maxIndexNumberRozrobka != minIndexNumberRozrobka)
					{
						int maxSum = _rozrobka[maxIndexNumberRozrobka];

						if (maxSum > _minNumber)
						{
							int maxSumFlip = (int)(sumFlip * randomPrecent / 100);

							if (maxSum - maxSumFlip >= _minNumber)
							{
								if (maxSum - maxSumFlip >= 0)
								{
									_rozrobka[maxIndexNumberRozrobka] = maxSum - maxSumFlip;
									_rozrobka[minIndexNumberRozrobka] = minSum + maxSumFlip;
								}
								else
								{
									_rozrobka[minIndexNumberRozrobka] += _rozrobka[maxIndexNumberRozrobka];
									_rozrobka[maxIndexNumberRozrobka] = 0;
								}

								isFlip = true;
							}
						}
					}

					if (!isFlip)
					{
						if (maxIndexNumberPidtrimka != -1 && _isTossOutPidtrimka)
						{
							int maxSum = _pidtrimka[maxIndexNumberPidtrimka];
							int maxSumFlip = (int)(sumFlip * randomPrecent / 100);
							if (maxSum > minNumber && _pidtrimka[maxIndexNumberPidtrimka] - maxSumFlip >= 0)
							{

								_pidtrimka[maxIndexNumberPidtrimka] -= maxSumFlip;
								_rozrobka[minIndexNumberRozrobka] += maxSumFlip;
							}
							else
							{
								_rozrobka[minIndexNumberRozrobka] += _pidtrimka[maxIndexNumberPidtrimka];
								_pidtrimka[maxIndexNumberPidtrimka] = 0;
							}

							isFlip = true;
						}
						else if (_isRemoveLinePidtrimka)
						{
							if (maxIndexNumberRozrobka != -1 && maxIndexNumberRozrobka != minIndexNumberRozrobka)
							{
								if (_rozrobka[minIndexNumberRozrobka] < _minNumber)
								{
									_rozrobka[maxIndexNumberRozrobka] += _rozrobka[minIndexNumberRozrobka];
									_rozrobka.RemoveAt(minIndexNumberRozrobka);
									isFlip = true;
								}
							}
						}
					}

					if (isFlip)
					{
						FindMaxIndex(_rozrobka, ref maxIndexNumberRozrobka);
						FindMinIndex(_rozrobka, ref minIndexNumberRozrobka);
						FindMaxIndex(_pidtrimka, ref maxIndexNumberPidtrimka);
						counterFlip--;
					}

					if (counterFlip <= 0)
						break;
				}
			}

			// Пересчет одинаковых сумм
			if (_isRemoveIdenticalNumbers)
			{
				RemoveIdenticalNumbers(ref _rozrobka, false, _minNumber);
				if (_isTossOutPidtrimka)
				{
					RemoveIdenticalNumbers(ref _pidtrimka, false, _minNumber);
				}
			}
		}

		/// <summary>
		/// Перекидывает суммы пидтрымки чтобы они были МЕНЬШЕ чем "_maxNumber"
		/// </summary>
		/// <param name="_maxNumber">Сумма меньше которой нужно сделать пункты</param>
		/// <param name="_isTossInRozrobka">Чтобы в конце сортировки, всё что лишнее перекидыввалось в розробку</param>
		/// <param name="_isRemoveIdenticalNumbers">Чтобы суммы пунктов не повторялись</param>
		/// <param name="_isAddNewPidtrimka">Чтобы излишки не перекидывались в розробку, а добавлялся пункт в дообробке</param>
		/// <param name="_countEnableWorks">Максимальное количество добавлений новых пунктов</param>
		public static void LessNumber(ref List<int> _rozrobka, ref List<int> _pidtrimka, int _maxNumber, bool _isTossInRozrobka, bool _isRemoveIdenticalNumbers, bool _isAddNewPidtrimka, int _countEnableWorks)
		{
			//ПЕРЕКИДЫВАЕТ суммы пидтрымки чтобы они были МЕНЬШЕ чем _maxNumber
			Random rnd = new Random();
			int percent = 10; // Какой процент от суммы перекидывать

			float randomPrecent; // Смещение процента от percent
			int minPercent = -3; // Погрешность от percent
			int maxPercent = 5; // Погрешность от percent

			if (percent + minPercent < 0)
				minPercent = -percent + 1;

			int maxIndexNumberPidtrimka = -1;
			FindMaxIndex(_pidtrimka, ref maxIndexNumberPidtrimka); // Индекс максимальной суммы пидтрымкы
			int minIndexNumberPidtrimka = -1;
			FindMinIndex(_pidtrimka, ref minIndexNumberPidtrimka); // Индекс минимальной суммы пидтрымкы

			int minIndexNumberRozrobka = -1;
			FindMinIndex(_rozrobka, ref minIndexNumberRozrobka); // Индекс минимальной суммы розробкы

			int sumFlip = 300; // Сколько будем перекидывать на другой пукнт

			bool isFlip = true;

			// Ограничение перебросов чтобы не зациклилось
			int totalSum = 0;
			foreach (int sum in _pidtrimka)
			{
				totalSum += sum;
			}

			foreach (int sum in _rozrobka)
			{
				totalSum += sum;
			}

			int counterFlip = (int)(totalSum * 1.5f) / (sumFlip * (percent + minPercent) / 100);

			while (isFlip)
			{
				isFlip = false;

				if (maxIndexNumberPidtrimka != -1)
				{
					int maxSum = _pidtrimka[maxIndexNumberPidtrimka];
					// Если максимальная сумма меньше чем ограничение то не нужно перекидывать
					if (maxSum <= _maxNumber)
						break;

					// Высчитывает рандомный процент от "minPercent" до "maxPercent" и прибавляет к "percent". Чтобы на каждый раз была разная сумма
					randomPrecent = percent + ((float)rnd.NextDouble() * (maxPercent + (minPercent * -1)) + minPercent);

					if (minIndexNumberPidtrimka != -1 && maxIndexNumberPidtrimka != minIndexNumberPidtrimka)
					{
						int minSum = _pidtrimka[minIndexNumberPidtrimka];

						// Если минимальная сумма меньше чем ограничение то не нужно перекидывать
						if (minSum < _maxNumber)
						{
							int minSumFlip = (int)(sumFlip * randomPrecent / 100);

							if (minSumFlip + minSum <= _maxNumber)
							{
								// Если после перекидывания сумма максимального пункта будет меньше 0 то всё из максимального пункта
								if (maxSum - minSumFlip >= 0)
								{
									_pidtrimka[maxIndexNumberPidtrimka] = maxSum - minSumFlip;
									_pidtrimka[minIndexNumberPidtrimka] = minSum + minSumFlip;
								}
								else
								{
									_pidtrimka[minIndexNumberPidtrimka] += maxSum;
									_pidtrimka[maxIndexNumberPidtrimka] = 0;
								}
								isFlip = true;
							}
						}
					}

					if (!isFlip)
					{
						if (_isAddNewPidtrimka && _countEnableWorks > 0)
						{
							// Добавляет пустой пункт с 0
							_pidtrimka.Add(0);
							isFlip = true;
							--_countEnableWorks;
						}
						else if (minIndexNumberRozrobka != -1 && _isTossInRozrobka)
						{
							// Перекидывает рандомную сумму излишка из пидтрымки в розробку
							int minSumFlip = (int)(sumFlip * randomPrecent / 100);

							if (maxSum - minSumFlip >= 0)
							{
								_pidtrimka[maxIndexNumberPidtrimka] = maxSum - minSumFlip;
								_rozrobka[minIndexNumberRozrobka] += minSumFlip;
							}
							else
							{
								_rozrobka[minIndexNumberRozrobka] += _pidtrimka[maxIndexNumberPidtrimka];
								_pidtrimka[maxIndexNumberPidtrimka] = 0;
							}
							isFlip = true;
						}
					}

					if (isFlip)
					{
						// Пересчет мин-макс индексов сумм
						FindMaxIndex(_pidtrimka, ref maxIndexNumberPidtrimka);
						FindMinIndex(_pidtrimka, ref minIndexNumberPidtrimka);
						FindMinIndex(_rozrobka, ref minIndexNumberRozrobka);

						counterFlip--;
					}

					if (counterFlip <= 0)
						break;
				}
			}

			// Пересчет одинаковых сумм
			if (_isRemoveIdenticalNumbers)
			{
				if (_isTossInRozrobka)
				{
					RemoveIdenticalNumbers(ref _rozrobka, true, _maxNumber);
				}
				RemoveIdenticalNumbers(ref _pidtrimka, true, _maxNumber);
			}
		}

		/// <summary>
		/// Если разница между суммами меньше чем "sumSpread" то перебрасывает эту разницу и процент от неё для рандома 0..1
		/// </summary>
		private static void RemoveIdenticalNumbers(ref List<int> _array, bool _isLess, int _opornSum)
		{
			List<KeyValuePair<int, List<int>>> pairs = new List<KeyValuePair<int, List<int>>>();
			for (int i = 0; i < _array.Count; ++i)
			{
				bool havePair = false;
				for (int j = 0; j < pairs.Count; ++j)
				{
					if (_array[i] == pairs[j].Key)
					{
						havePair = true;
						break;
					}
				}
				if (havePair) continue;

				KeyValuePair<int, List<int>> pair = new KeyValuePair<int, List<int>>(_array[i], new List<int>() { i });
				for (int j = i + 1; j < _array.Count; ++j)
				{
					if (_array[i] == _array[j])
					{
						pair.Value.Add(j);
					}
				}
				if (pair.Value.Count > 1)
				{
					pairs.Add(pair);
				}
			}

			Random random = new Random();
			int dir = _isLess ? -1 : 1;
			int prevPairCount = -1;
			for (int counter = _array.Count; pairs.Count != 0 && counter > 0;)
			{
				int maxInd = -1;
				if (_isLess)
				{
					FindMinIndex(_array, ref maxInd);
				}
				else
				{
					FindMaxIndex(_array, ref maxInd);
				}
				int rndStep = random.Next(1, 9) * dir;

				for (int i = 1; i < pairs[0].Value.Count; ++i)
				{
					int nextMaxSum = _array[maxInd] - (i * rndStep);

					if ((_isLess && nextMaxSum >= _opornSum)
					|| (!_isLess && nextMaxSum <= _opornSum))
						continue;

					_array[maxInd] = nextMaxSum;
					_array[pairs[0].Value[i]] += i * rndStep;
				}

				UpdateIndexOnPairs(_array, pairs, maxInd);

				for (int i = 1; pairs.Count > 0 && i < pairs[0].Value.Count; ++i)
				{
					UpdateIndexOnPairs(_array, pairs, pairs[0].Value[i]);
				}

				if (prevPairCount == pairs.Count)
				{
					--counter;
				}
				prevPairCount = pairs.Count;
			}
		}

		private static void UpdateIndexOnPairs(List<int> _array, List<KeyValuePair<int, List<int>>> pairs, int index)
		{
			bool pairCreated = false;
			for (int i = 0; i < pairs.Count; ++i)
			{
				if (pairs[i].Value.Contains(index))
				{
					pairs[i].Value.Remove(index);

					if (pairs[i].Value.Count < 2)
					{
						pairs.RemoveAt(i);
					}
					break;
				}
			}

			for (int i = 0; i < pairs.Count; ++i)
			{
				if (pairs[i].Key == _array[index])
				{
					pairs[i].Value.Add(index);
					pairCreated = true;
					break;
				}
			}

			if (!pairCreated)
			{
				KeyValuePair<int, List<int>> pair = new KeyValuePair<int, List<int>>(_array[index], new List<int> { index });
				for (int i = 0; i < _array.Count; ++i)
				{
					if (i != index && pair.Key == _array[i])
					{
						pair.Value.Add(i);
					}
				}
				if (pair.Value.Count > 1)
				{
					pairs.Add(pair);
				}
			}
		}

		private static void FindMaxIndex(List<int> _array, ref int _maxIndexNumber)
		{
			int maxSum = int.MinValue;
			for (int i = 0; i < _array.Count; i++)
			{
				int sum = _array[i];
				if (maxSum < sum)
				{
					maxSum = sum;
					_maxIndexNumber = i;
				}
			}
		}

		private static void FindMinIndex(List<int> _array, ref int _minIndexNumber)
		{
			int minSum = int.MaxValue;
			for (int i = 0; i < _array.Count; i++)
			{
				int sum = _array[i];
				if (minSum >= sum)
				{
					minSum = sum;
					_minIndexNumber = i;
				}
			}
		}
	}
}
