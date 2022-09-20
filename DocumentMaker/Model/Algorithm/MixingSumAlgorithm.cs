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

			int maxSumFlip = minNumber / 2; // Сколько будем перекидывать на другой пукнт

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

			int counterFlip = (int)(totalSum * 1.5f) / maxSumFlip;

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
							maxSumFlip = (int)(maxSum * randomPrecent / 100);

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
							else
							{
								maxSumFlip = (int)(minNumber * (randomPrecent * 5) / 100);

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
							}
						}
					}

					if (!isFlip)
					{
						if (maxIndexNumberPidtrimka != -1 && _isTossOutPidtrimka)
						{
							int maxSum = _pidtrimka[maxIndexNumberPidtrimka];
							maxSumFlip = (int)(maxSum * randomPrecent / 100);
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
				RemoveIdenticalNumbers(ref _rozrobka);
				RemoveIdenticalNumbers(ref _pidtrimka);
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

			int minSumFlip = _maxNumber * (percent - minPercent) / 100; // Сколько будем перекидывать на другой пукнт

			bool isFlip = true;

			// Ограничение перебросов чтобы не зациклилось
			int totalSum = 0;
			foreach(int sum in _pidtrimka)
			{
				totalSum += sum;
			}

			foreach (int sum in _rozrobka)
			{
				totalSum += sum;
			}

			int counterFlip = (int)(totalSum * 1.5f) / minSumFlip;

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
							minSumFlip = (int)(maxSum * randomPrecent / 100);

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
							else
							{
								minSumFlip = (int)(_maxNumber * randomPrecent / 100);

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
							minSumFlip = (int)(maxSum * randomPrecent / 100);

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
				RemoveIdenticalNumbers(ref _rozrobka);
				RemoveIdenticalNumbers(ref _pidtrimka);
			}
		}

		/// <summary>
		/// Если разница между суммами меньше чем "sumSpread" то перебрасывает эту разницу и процент от неё для рандома 0..1
		/// </summary>
		private static void RemoveIdenticalNumbers(ref List<int> _array)
		{
			Random rnd = new Random();
			int sumSpread = 75; // Разница между пунктами
			int percent = 100; // Какой процент от суммы перекидывать 
			int minPercent = 3 * 100 / sumSpread; // Погрешность от percent
			int maxPercent = 100; // Погрешность от percent
			float randomPrecent; // Смещение процента от percent

			if (percent + minPercent < 0)
				minPercent = -percent;

			for (int i = 0; i < _array.Count; i++)
			{
				if (_array[i] == 0)
					continue;

				for (int j = 0; j < _array.Count; j++)
				{
					if (i != j)
					{
						if (_array[j] == 0)
							continue;

						// Если разница в суммах меньше чем "sumSpread" то перекидывает "sumSpread + рандомную сумму от sumSpread"
						int sumDiff = Math.Abs(_array[j] - _array[i]);
						if (sumDiff <= sumSpread)
						{
							randomPrecent = percent + ((float)rnd.NextDouble() * (maxPercent + (minPercent * -1)) + minPercent);
							int sumFlip = (int)(sumSpread * randomPrecent / 100);

							if (_array[i] - sumFlip >= 0)
							{
								_array[i] -= sumFlip;
								_array[j] += sumFlip;
								break;
							}
							else
							{
								_array[j] += _array[i];
								_array[i] = 0;
								break;
							}
						}
					}
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
