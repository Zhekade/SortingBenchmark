using System;
using System.Runtime.InteropServices;

namespace SortingRunner
{
    public static class SortingAlgorithms
    {
        // -------------------------------------------------------------
        // C# Реалізації
        // -------------------------------------------------------------
        public static void BubbleSortCSharp(int[] arr, out long comparisons, out long swaps)
        {
            comparisons = 0;
            swaps = 0;
            int n = arr.Length;
            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 0; j < n - i - 1; j++)
                {
                    comparisons++;
                    if (arr[j] > arr[j + 1])
                    {
                        swaps++;
                        int temp = arr[j];
                        arr[j] = arr[j + 1];
                        arr[j + 1] = temp;
                    }
                }
            }
        }

        private static void QuickSortCSharp_Helper(int[] arr, int low, int high, ref long comparisons, ref long swaps)
        {
            if (low < high)
            {
                int pivot = arr[high];
                int i = (low - 1);

                for (int j = low; j <= high - 1; j++)
                {
                    comparisons++;
                    if (arr[j] < pivot)
                    {
                        i++;
                        if (i != j)
                        {
                            swaps++;
                            int temp = arr[i];
                            arr[i] = arr[j];
                            arr[j] = temp;
                        }
                    }
                }

                i++;
                if (i != high)
                {
                    swaps++;
                    int temp2 = arr[i];
                    arr[i] = arr[high];
                    arr[high] = temp2;
                }

                int pi = i;

                QuickSortCSharp_Helper(arr, low, pi - 1, ref comparisons, ref swaps);
                QuickSortCSharp_Helper(arr, pi + 1, high, ref comparisons, ref swaps);
            }
        }

        public static void QuickSortCSharp(int[] arr, out long comparisons, out long swaps)
        {
            comparisons = 0;
            swaps = 0;
            QuickSortCSharp_Helper(arr, 0, arr.Length - 1, ref comparisons, ref swaps);
        }

        // -------------------------------------------------------------
        // P/Invoke для C та C++ з SortingCore.dll
        // -------------------------------------------------------------
        // DllImport шукатиме SortingCore.dll у директорії завантаження програми
        
        [DllImport("SortingCore.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void BubbleSortC(int[] arr, int size, out long comparisons, out long swaps);

        [DllImport("SortingCore.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void QuickSortC(int[] arr, int size, out long comparisons, out long swaps);

        [DllImport("SortingCore.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void BubbleSortCpp(int[] arr, int size, out long comparisons, out long swaps);

        [DllImport("SortingCore.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void QuickSortCpp(int[] arr, int size, out long comparisons, out long swaps);
    }
}
