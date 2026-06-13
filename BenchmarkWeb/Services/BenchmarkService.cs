using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using SortingRunner;

namespace BenchmarkWeb.Services
{
    public class BenchmarkConfig
    {
        public List<string> Languages { get; set; } = new();
        public List<string> Algorithms { get; set; } = new();
        public List<string> States { get; set; } = new();
        public List<int> Sizes { get; set; } = new();
        public int Runs { get; set; } = 5;
    }

    public class BenchmarkResult
    {
        public string Language { get; set; } = "";
        public string Algorithm { get; set; } = "";
        public string State { get; set; } = "";
        public int Size { get; set; }
        public double TimeMs { get; set; }
        public long Comparisons { get; set; }
        public long Swaps { get; set; }
        public string Error { get; set; } = "";
    }

    public class BenchmarkService
    {
        private CancellationTokenSource? _cts;
        private readonly ConcurrentQueue<string> _eventsQueue = new();
        private Task? _benchmarkTask;
        public bool IsRunning => _benchmarkTask != null && !_benchmarkTask.IsCompleted;

        public void StartBenchmark(BenchmarkConfig config)
        {
            if (IsRunning) StopBenchmark();

            _cts = new CancellationTokenSource();
            _eventsQueue.Clear();
            
            _eventsQueue.Enqueue(JsonSerializer.Serialize(new { type = "status", message = "Started" }));

            _benchmarkTask = Task.Run(() => RunBenchmarks(config, _cts.Token));
        }

        public void StopBenchmark()
        {
            if (_cts != null && !_cts.IsCancellationRequested)
            {
                _cts.Cancel();
                _eventsQueue.Enqueue(JsonSerializer.Serialize(new { type = "status", message = "Stopped" }));
            }
        }

        public async IAsyncEnumerable<string> GetEventStream([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_eventsQueue.TryDequeue(out var ev))
                {
                    yield return ev;
                }
                else
                {
                    if (!IsRunning) 
                    {
                        await Task.Delay(1000, cancellationToken);
                        continue;
                    }
                    await Task.Delay(100, cancellationToken);
                }
            }
        }

        private void RunBenchmarks(BenchmarkConfig config, CancellationToken token)
        {
            try
            {
                // Warm-up
                try {
                    int[] warmUpArr = ArrayGenerator.GenerateRandom(1000);
                    SortingAlgorithms.BubbleSortCSharp(ArrayGenerator.CopyArray(warmUpArr), out _, out _);
                    SortingAlgorithms.SelectionSortCSharp(ArrayGenerator.CopyArray(warmUpArr), out _, out _);
                    SortingAlgorithms.InsertionSortCSharp(ArrayGenerator.CopyArray(warmUpArr), out _, out _);
                    SortingAlgorithms.QuickSortCSharp(ArrayGenerator.CopyArray(warmUpArr), out _, out _);
                    SortingAlgorithms.MergeSortCSharp(ArrayGenerator.CopyArray(warmUpArr), out _, out _);

                    SortingAlgorithms.BubbleSortC(ArrayGenerator.CopyArray(warmUpArr), 1000, out _, out _);
                    SortingAlgorithms.SelectionSortC(ArrayGenerator.CopyArray(warmUpArr), 1000, out _, out _);
                    SortingAlgorithms.InsertionSortC(ArrayGenerator.CopyArray(warmUpArr), 1000, out _, out _);
                    SortingAlgorithms.QuickSortC(ArrayGenerator.CopyArray(warmUpArr), 1000, out _, out _);
                    SortingAlgorithms.MergeSortC(ArrayGenerator.CopyArray(warmUpArr), 1000, out _, out _);

                    SortingAlgorithms.BubbleSortCpp(ArrayGenerator.CopyArray(warmUpArr), 1000, out _, out _);
                    SortingAlgorithms.SelectionSortCpp(ArrayGenerator.CopyArray(warmUpArr), 1000, out _, out _);
                    SortingAlgorithms.InsertionSortCpp(ArrayGenerator.CopyArray(warmUpArr), 1000, out _, out _);
                    SortingAlgorithms.QuickSortCpp(ArrayGenerator.CopyArray(warmUpArr), 1000, out _, out _);
                    SortingAlgorithms.MergeSortCpp(ArrayGenerator.CopyArray(warmUpArr), 1000, out _, out _);
                } catch { /* Ignore missing DLL during warmup */ }

                foreach (var size in config.Sizes)
                {
                    if (token.IsCancellationRequested) break;

                    int[] randomBase = ArrayGenerator.GenerateRandom(size);
                    int[] sortedBase = ArrayGenerator.GenerateSorted(size);
                    int[] reversedBase = ArrayGenerator.GenerateReversed(size);

                    foreach (var state in config.States)
                    {
                        if (token.IsCancellationRequested) break;

                        int[] baseArr = state switch
                        {
                            "Sorted" => sortedBase,
                            "Reversed" => reversedBase,
                            _ => randomBase
                        };

                        foreach (var algo in config.Algorithms)
                        {
                            if (token.IsCancellationRequested) break;
                            
                            bool isSlowAlgorithm = (algo == "BubbleSort" || algo == "SelectionSort" || algo == "InsertionSort");
                            if (isSlowAlgorithm && size >= 100_000)
                            {
                                foreach (var lang in config.Languages)
                                {
                                    var skippedRes = new BenchmarkResult 
                                    { 
                                        Language = lang, Algorithm = algo, State = state, Size = size, Error = "ПРОПУЩЕНО (запобіжник спрацював для великого об'єму даних)" 
                                    };
                                    _eventsQueue.Enqueue(JsonSerializer.Serialize(new { type = "result", data = skippedRes }));
                                }
                                continue;
                            }

                            foreach (var lang in config.Languages)
                            {
                                if (token.IsCancellationRequested) break;

                                var res = RunSingle(lang, algo, state, size, baseArr, config.Runs, token);
                                if (res != null)
                                {
                                    _eventsQueue.Enqueue(JsonSerializer.Serialize(new { type = "result", data = res }));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                 _eventsQueue.Enqueue(JsonSerializer.Serialize(new { type = "error", message = ex.Message }));
            }
            finally
            {
                if (!token.IsCancellationRequested)
                    _eventsQueue.Enqueue(JsonSerializer.Serialize(new { type = "status", message = "Completed" }));
            }
        }

        private BenchmarkResult? RunSingle(string language, string algorithm, string state, int size, int[] baseArr, int runs, CancellationToken token)
        {
            long totalTimeMs = 0;
            long lastComparisons = 0;
            long lastSwaps = 0;

            for (int i = 0; i < runs; i++)
            {
                if (token.IsCancellationRequested) return null;

                GC.Collect();
                GC.WaitForPendingFinalizers();

                int[] testArr = ArrayGenerator.CopyArray(baseArr);
                Stopwatch sw = Stopwatch.StartNew();

                try
                {
                    if (language == "C#")
                    {
                        if (algorithm == "BubbleSort") SortingAlgorithms.BubbleSortCSharp(testArr, out lastComparisons, out lastSwaps);
                        else if (algorithm == "SelectionSort") SortingAlgorithms.SelectionSortCSharp(testArr, out lastComparisons, out lastSwaps);
                        else if (algorithm == "InsertionSort") SortingAlgorithms.InsertionSortCSharp(testArr, out lastComparisons, out lastSwaps);
                        else if (algorithm == "QuickSort") SortingAlgorithms.QuickSortCSharp(testArr, out lastComparisons, out lastSwaps);
                        else if (algorithm == "MergeSort") SortingAlgorithms.MergeSortCSharp(testArr, out lastComparisons, out lastSwaps);
                    }
                    else if (language == "C")
                    {
                        if (algorithm == "BubbleSort") SortingAlgorithms.BubbleSortC(testArr, size, out lastComparisons, out lastSwaps);
                        else if (algorithm == "SelectionSort") SortingAlgorithms.SelectionSortC(testArr, size, out lastComparisons, out lastSwaps);
                        else if (algorithm == "InsertionSort") SortingAlgorithms.InsertionSortC(testArr, size, out lastComparisons, out lastSwaps);
                        else if (algorithm == "QuickSort") SortingAlgorithms.QuickSortC(testArr, size, out lastComparisons, out lastSwaps);
                        else if (algorithm == "MergeSort") SortingAlgorithms.MergeSortC(testArr, size, out lastComparisons, out lastSwaps);
                    }
                    else if (language == "C++")
                    {
                        if (algorithm == "BubbleSort") SortingAlgorithms.BubbleSortCpp(testArr, size, out lastComparisons, out lastSwaps);
                        else if (algorithm == "SelectionSort") SortingAlgorithms.SelectionSortCpp(testArr, size, out lastComparisons, out lastSwaps);
                        else if (algorithm == "InsertionSort") SortingAlgorithms.InsertionSortCpp(testArr, size, out lastComparisons, out lastSwaps);
                        else if (algorithm == "QuickSort") SortingAlgorithms.QuickSortCpp(testArr, size, out lastComparisons, out lastSwaps);
                        else if (algorithm == "MergeSort") SortingAlgorithms.MergeSortCpp(testArr, size, out lastComparisons, out lastSwaps);
                    }
                }
                catch (DllNotFoundException)
                {
                    return new BenchmarkResult { Language = language, Algorithm = algorithm, State = state, Size = size, Error = "SortingCore.dll not found" };
                }

                sw.Stop();
                totalTimeMs += sw.ElapsedMilliseconds;
            }

            return new BenchmarkResult
            {
                Language = language,
                Algorithm = algorithm,
                State = state,
                Size = size,
                TimeMs = (double)totalTimeMs / runs,
                Comparisons = lastComparisons,
                Swaps = lastSwaps
            };
        }
    }
}
