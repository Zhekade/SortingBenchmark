using System;
using System.Diagnostics;
using System.IO;

namespace SortingRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Запуск тестового стенду алгоритмів сортування...");

            // Етап 1: Прогрів (Warm-up)
            WarmUp();

            // Підготовка до тестування
            int[] sizes = { 10_000, 100_000, 1_000_000 };
            string[] states = { "Random", "Sorted", "Reversed" };
            int testRuns = 10; // Кількість запусків для усереднення

            string csvFilePath = "results.csv";
            using (StreamWriter writer = new StreamWriter(csvFilePath))
            {
                writer.WriteLine("Language,Algorithm,ArrayState,Size,TimeMs,Comparisons,Swaps");

                foreach (int size in sizes)
                {
                    Console.WriteLine($"\nГенерація масивів розміром {size}...");
                    
                    int[] randomBase = ArrayGenerator.GenerateRandom(size);
                    int[] sortedBase = ArrayGenerator.GenerateSorted(size);
                    int[] reversedBase = ArrayGenerator.GenerateReversed(size);

                    int[][] baseArrays = { randomBase, sortedBase, reversedBase };

                    for (int stateIndex = 0; stateIndex < states.Length; stateIndex++)
                    {
                        string state = states[stateIndex];
                        int[] baseArr = baseArrays[stateIndex];

                        // Тут перелік всіх алгоритмів, які ми тестуємо. 
                        // ВАЖЛИВО: для BubbleSort 1_000_000 елементів займе занадто багато часу (O(N^2)),
                        // тому ми можемо обмежити його або чекати. Для цілей стенду, ми його пропустимо для мільйона.
                        bool skipBubble = (size >= 1_000_000);

                        if (!skipBubble)
                        {
                            RunBenchmark("C#", "BubbleSort", state, size, baseArr, testRuns, writer,
                                (arr, out long comp, out long swap) => SortingAlgorithms.BubbleSortCSharp(arr, out comp, out swap));

                            RunBenchmark("C", "BubbleSort", state, size, baseArr, testRuns, writer,
                                (arr, out long comp, out long swap) => SortingAlgorithms.BubbleSortC(arr, arr.Length, out comp, out swap));

                            RunBenchmark("C++", "BubbleSort", state, size, baseArr, testRuns, writer,
                                (arr, out long comp, out long swap) => SortingAlgorithms.BubbleSortCpp(arr, arr.Length, out comp, out swap));
                        }
                        else
                        {
                            Console.WriteLine($"[C#, C, C++] BubbleSort для розміру {size} ({state}) пропущено (занадто довго).");
                        }

                        RunBenchmark("C#", "QuickSort", state, size, baseArr, testRuns, writer,
                            (arr, out long comp, out long swap) => SortingAlgorithms.QuickSortCSharp(arr, out comp, out swap));

                        RunBenchmark("C", "QuickSort", state, size, baseArr, testRuns, writer,
                            (arr, out long comp, out long swap) => SortingAlgorithms.QuickSortC(arr, arr.Length, out comp, out swap));

                        RunBenchmark("C++", "QuickSort", state, size, baseArr, testRuns, writer,
                            (arr, out long comp, out long swap) => SortingAlgorithms.QuickSortCpp(arr, arr.Length, out comp, out swap));
                    }
                }
            }

            Console.WriteLine($"\nТестування завершено. Результати збережено у {csvFilePath}");
            Console.WriteLine("Натисніть Enter для виходу...");
            Console.ReadLine();
        }

        delegate void SortAction(int[] arr, out long comparisons, out long swaps);

        static void RunBenchmark(string language, string algorithm, string state, int size, int[] baseArr, int runs, StreamWriter csvWriter, SortAction sortAction)
        {
            Console.Write($"[{language}] {algorithm} - {state} [{size}]: ");
            
            long totalTimeMs = 0;
            long lastComparisons = 0;
            long lastSwaps = 0;

            for (int i = 0; i < runs; i++)
            {
                // Очищення пам'яті перед кожним запуском для чистоти експерименту
                GC.Collect();
                GC.WaitForPendingFinalizers();

                int[] testArr = ArrayGenerator.CopyArray(baseArr);

                Stopwatch sw = Stopwatch.StartNew();
                
                try
                {
                    sortAction(testArr, out lastComparisons, out lastSwaps);
                }
                catch (DllNotFoundException)
                {
                    if (i == 0)
                    {
                        Console.WriteLine($"ПОМИЛКА: Не знайдено SortingCore.dll! Переконайтеся, що DLL скомпільована і лежить у папці з EXE.");
                        return; // Зупиняємо цей бенчмарк
                    }
                }

                sw.Stop();
                totalTimeMs += sw.ElapsedMilliseconds;
            }

            double averageTimeMs = (double)totalTimeMs / runs;
            
            Console.WriteLine($"{averageTimeMs:F2} ms (Порівнянь: {lastComparisons}, Обмінів: {lastSwaps})");
            csvWriter.WriteLine($"{language},{algorithm},{state},{size},{averageTimeMs.ToString(System.Globalization.CultureInfo.InvariantCulture)},{lastComparisons},{lastSwaps}");
        }

        static void WarmUp()
        {
            Console.WriteLine("Виконується прогрів (Warm-up)...");
            int warmUpSize = 1000;
            int[] arr = ArrayGenerator.GenerateRandom(warmUpSize);
            long comp, swap;

            // Прогріваємо C#
            SortingAlgorithms.BubbleSortCSharp(ArrayGenerator.CopyArray(arr), out comp, out swap);
            SortingAlgorithms.QuickSortCSharp(ArrayGenerator.CopyArray(arr), out comp, out swap);

            // Прогріваємо C та C++
            try
            {
                SortingAlgorithms.BubbleSortC(ArrayGenerator.CopyArray(arr), warmUpSize, out comp, out swap);
                SortingAlgorithms.QuickSortC(ArrayGenerator.CopyArray(arr), warmUpSize, out comp, out swap);
                SortingAlgorithms.BubbleSortCpp(ArrayGenerator.CopyArray(arr), warmUpSize, out comp, out swap);
                SortingAlgorithms.QuickSortCpp(ArrayGenerator.CopyArray(arr), warmUpSize, out comp, out swap);
            }
            catch (DllNotFoundException)
            {
                Console.WriteLine("ПОПЕРЕДЖЕННЯ: Під час прогріву не знайдено SortingCore.dll. Вона завантажиться пізніше, якщо лежить у правильній папці, але P/Invoke методи не прогріті.");
            }

            Console.WriteLine("Прогрів завершено.");
        }
    }
}
