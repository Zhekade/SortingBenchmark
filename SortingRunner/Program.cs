using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace SortingRunner
{
    class BenchmarkResult 
    {
        public string Language { get; set; }
        public string Algorithm { get; set; }
        public string State { get; set; }
        public int Size { get; set; }
        public double TimeMs { get; set; }
        public bool Skipped { get; set; }
        public string SkipReason { get; set; }
    }

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
            List<BenchmarkResult> results = new List<BenchmarkResult>();

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

                        results.Add(RunBenchmark("C#", "BubbleSort", state, size, baseArr, testRuns, writer,
                            (int[] arr, out long comp, out long swap) => SortingAlgorithms.BubbleSortCSharp(arr, out comp, out swap), isSlowAlgorithm: true));

                        results.Add(RunBenchmark("C", "BubbleSort", state, size, baseArr, testRuns, writer,
                            (int[] arr, out long comp, out long swap) => SortingAlgorithms.BubbleSortC(arr, arr.Length, out comp, out swap), isSlowAlgorithm: true));

                        results.Add(RunBenchmark("C++", "BubbleSort", state, size, baseArr, testRuns, writer,
                            (int[] arr, out long comp, out long swap) => SortingAlgorithms.BubbleSortCpp(arr, arr.Length, out comp, out swap), isSlowAlgorithm: true));

                        results.Add(RunBenchmark("C#", "SelectionSort", state, size, baseArr, testRuns, writer,
                            (int[] arr, out long comp, out long swap) => SortingAlgorithms.SelectionSortCSharp(arr, out comp, out swap), isSlowAlgorithm: true));

                        results.Add(RunBenchmark("C", "SelectionSort", state, size, baseArr, testRuns, writer,
                            (int[] arr, out long comp, out long swap) => SortingAlgorithms.SelectionSortC(arr, arr.Length, out comp, out swap), isSlowAlgorithm: true));

                        results.Add(RunBenchmark("C++", "SelectionSort", state, size, baseArr, testRuns, writer,
                            (int[] arr, out long comp, out long swap) => SortingAlgorithms.SelectionSortCpp(arr, arr.Length, out comp, out swap), isSlowAlgorithm: true));

                        results.Add(RunBenchmark("C#", "InsertionSort", state, size, baseArr, testRuns, writer,
                            (int[] arr, out long comp, out long swap) => SortingAlgorithms.InsertionSortCSharp(arr, out comp, out swap), isSlowAlgorithm: true));

                        results.Add(RunBenchmark("C", "InsertionSort", state, size, baseArr, testRuns, writer,
                            (int[] arr, out long comp, out long swap) => SortingAlgorithms.InsertionSortC(arr, arr.Length, out comp, out swap), isSlowAlgorithm: true));

                        results.Add(RunBenchmark("C++", "InsertionSort", state, size, baseArr, testRuns, writer,
                            (int[] arr, out long comp, out long swap) => SortingAlgorithms.InsertionSortCpp(arr, arr.Length, out comp, out swap), isSlowAlgorithm: true));

                        results.Add(RunBenchmark("C#", "QuickSort", state, size, baseArr, testRuns, writer,
                            (int[] arr, out long comp, out long swap) => SortingAlgorithms.QuickSortCSharp(arr, out comp, out swap)));

                        results.Add(RunBenchmark("C", "QuickSort", state, size, baseArr, testRuns, writer,
                            (int[] arr, out long comp, out long swap) => SortingAlgorithms.QuickSortC(arr, arr.Length, out comp, out swap)));

                        results.Add(RunBenchmark("C++", "QuickSort", state, size, baseArr, testRuns, writer,
                            (int[] arr, out long comp, out long swap) => SortingAlgorithms.QuickSortCpp(arr, arr.Length, out comp, out swap)));

                        results.Add(RunBenchmark("C#", "MergeSort", state, size, baseArr, testRuns, writer,
                            (int[] arr, out long comp, out long swap) => SortingAlgorithms.MergeSortCSharp(arr, out comp, out swap)));

                        results.Add(RunBenchmark("C", "MergeSort", state, size, baseArr, testRuns, writer,
                            (int[] arr, out long comp, out long swap) => SortingAlgorithms.MergeSortC(arr, arr.Length, out comp, out swap)));

                        results.Add(RunBenchmark("C++", "MergeSort", state, size, baseArr, testRuns, writer,
                            (int[] arr, out long comp, out long swap) => SortingAlgorithms.MergeSortCpp(arr, arr.Length, out comp, out swap)));
                    }
                }
            }

            PrintSummary(results);

            Console.WriteLine($"\nТестування завершено. Результати збережено у {csvFilePath}");
            Console.WriteLine("Натисніть Enter для виходу...");
            Console.ReadLine();
        }

        static void PrintSummary(List<BenchmarkResult> results)
        {
            Console.WriteLine("\n==========================================================================");
            Console.WriteLine("                          ПІДСУМКОВА ВІЗУАЛІЗАЦІЯ                         ");
            Console.WriteLine("==========================================================================");

            var groups = results.GroupBy(r => new { r.Algorithm, r.State, r.Size });

            foreach (var group in groups)
            {
                Console.WriteLine($"\n[ {group.Key.Algorithm} | {group.Key.State} | {group.Key.Size} елементів ]");
                
                var csharpResult = group.FirstOrDefault(r => r.Language == "C#");
                var cResult = group.FirstOrDefault(r => r.Language == "C");
                var cppResult = group.FirstOrDefault(r => r.Language == "C++");

                // Відображення абсолютного часу або причини пропуску
                foreach (var r in group)
                {
                    if (r.Skipped)
                    {
                        Console.WriteLine($"  {r.Language,4}: ПРОПУЩЕНО (Причина: {r.SkipReason})");
                    }
                    else
                    {
                        // Смуга (bar chart) для візуалізації абсолютної тривалості (1 символ = 10 ms для довгих або 1 ms для швидких)
                        // Щоб короткі тести (0-1 ms) теж мали смугу, масштабуємо динамічно або використовуємо логарифм.
                        // Використаємо просту лінійну шкалу: 1 символ = 5 ms (макс 40 символів). Для дуже малих часів хоча б 1 символ.
                        int barLength = (int)Math.Min(r.TimeMs / 5.0, 40);
                        if (barLength < 1 && r.TimeMs >= 0) barLength = 1;
                        string bar = new string('█', barLength);
                        
                        Console.WriteLine($"  {r.Language,4}: {r.TimeMs,8:F2} ms | {bar}");
                    }
                }

                // Порівняння відсоткового співвідношення швидкостей
                void Compare(BenchmarkResult r1, BenchmarkResult r2, string lang1, string lang2)
                {
                    if (r1 != null && !r1.Skipped && r2 != null && !r2.Skipped && r1.TimeMs > 0 && r2.TimeMs > 0)
                    {
                        double speedup = Math.Max(r1.TimeMs, r2.TimeMs) / Math.Min(r1.TimeMs, r2.TimeMs);
                        if (speedup > 1.05) // Якщо різниця більше 5%
                        {
                            string faster = r1.TimeMs > r2.TimeMs ? lang2 : lang1;
                            string slower = r1.TimeMs > r2.TimeMs ? lang1 : lang2;
                            Console.WriteLine($"  -> {faster} швидший за {slower} на {(speedup - 1) * 100:F1}% ({speedup:F2}x)");
                        }
                        else
                        {
                            Console.WriteLine($"  -> {lang1} та {lang2} мають приблизно однакову швидкість");
                        }
                    }
                }

                Compare(csharpResult, cResult, "C#", "C");
                Compare(csharpResult, cppResult, "C#", "C++");
            }
            Console.WriteLine("\n==========================================================================");
        }

        delegate void SortAction(int[] arr, out long comparisons, out long swaps);

        static BenchmarkResult RunBenchmark(string language, string algorithm, string state, int size, int[] baseArr, int runs, StreamWriter csvWriter, SortAction sortAction, bool isSlowAlgorithm = false)
        {
            if (isSlowAlgorithm && size >= 100_000)
            {
                string skipReason = "запобіжник спрацював для великого об'єму даних";
                Console.WriteLine($"[{language}] {algorithm} - {state} [{size}]: ПРОПУЩЕНО ({skipReason})");
                return new BenchmarkResult { Language = language, Algorithm = algorithm, State = state, Size = size, Skipped = true, SkipReason = skipReason };
            }

            Console.Write($"[{language}] {algorithm} - {state} [{size}]: ");
            
            long totalTimeMs = 0;
            long lastComparisons = 0;
            long lastSwaps = 0;

            for (int i = 0; i < runs; i++)
            {
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
                        Console.WriteLine($"ПОМИЛКА: Не знайдено SortingCore.dll!");
                        return new BenchmarkResult { Language = language, Algorithm = algorithm, State = state, Size = size, Skipped = true, SkipReason = "DLL не знайдено" };
                    }
                }

                sw.Stop();
                totalTimeMs += sw.ElapsedMilliseconds;
            }

            double averageTimeMs = (double)totalTimeMs / runs;
            
            Console.WriteLine($"{averageTimeMs:F2} ms (Порівнянь: {lastComparisons}, Обмінів: {lastSwaps})");
            csvWriter.WriteLine($"{language},{algorithm},{state},{size},{averageTimeMs.ToString(System.Globalization.CultureInfo.InvariantCulture)},{lastComparisons},{lastSwaps}");

            return new BenchmarkResult { Language = language, Algorithm = algorithm, State = state, Size = size, TimeMs = averageTimeMs };
        }

        static void WarmUp()
        {
            Console.WriteLine("Виконується прогрів (Warm-up)...");
            int warmUpSize = 1000;
            int[] arr = ArrayGenerator.GenerateRandom(warmUpSize);
            long comp, swap;

            SortingAlgorithms.BubbleSortCSharp(ArrayGenerator.CopyArray(arr), out comp, out swap);
            SortingAlgorithms.SelectionSortCSharp(ArrayGenerator.CopyArray(arr), out comp, out swap);
            SortingAlgorithms.InsertionSortCSharp(ArrayGenerator.CopyArray(arr), out comp, out swap);
            SortingAlgorithms.MergeSortCSharp(ArrayGenerator.CopyArray(arr), out comp, out swap);
            SortingAlgorithms.QuickSortCSharp(ArrayGenerator.CopyArray(arr), out comp, out swap);

            try
            {
                SortingAlgorithms.BubbleSortC(ArrayGenerator.CopyArray(arr), warmUpSize, out comp, out swap);
                SortingAlgorithms.SelectionSortC(ArrayGenerator.CopyArray(arr), warmUpSize, out comp, out swap);
                SortingAlgorithms.InsertionSortC(ArrayGenerator.CopyArray(arr), warmUpSize, out comp, out swap);
                SortingAlgorithms.MergeSortC(ArrayGenerator.CopyArray(arr), warmUpSize, out comp, out swap);
                SortingAlgorithms.QuickSortC(ArrayGenerator.CopyArray(arr), warmUpSize, out comp, out swap);
                
                SortingAlgorithms.BubbleSortCpp(ArrayGenerator.CopyArray(arr), warmUpSize, out comp, out swap);
                SortingAlgorithms.SelectionSortCpp(ArrayGenerator.CopyArray(arr), warmUpSize, out comp, out swap);
                SortingAlgorithms.InsertionSortCpp(ArrayGenerator.CopyArray(arr), warmUpSize, out comp, out swap);
                SortingAlgorithms.MergeSortCpp(ArrayGenerator.CopyArray(arr), warmUpSize, out comp, out swap);
                SortingAlgorithms.QuickSortCpp(ArrayGenerator.CopyArray(arr), warmUpSize, out comp, out swap);
            }
            catch (DllNotFoundException)
            {
                Console.WriteLine("ПОПЕРЕДЖЕННЯ: Під час прогріву не знайдено SortingCore.dll. Вона завантажиться пізніше.");
            }

            Console.WriteLine("Прогрів завершено.");
        }
    }
}
