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

        public static void SelectionSortCSharp(int[] arr, out long comparisons, out long swaps)
        {
            comparisons = 0;
            swaps = 0;
            int n = arr.Length;
            for (int i = 0; i < n - 1; i++)
            {
                int min_idx = i;
                for (int j = i + 1; j < n; j++)
                {
                    comparisons++;
                    if (arr[j] < arr[min_idx])
                    {
                        min_idx = j;
                    }
                }
                if (min_idx != i)
                {
                    swaps++;
                    int temp = arr[i];
                    arr[i] = arr[min_idx];
                    arr[min_idx] = temp;
                }
            }
        }

        public static void InsertionSortCSharp(int[] arr, out long comparisons, out long swaps)
        {
            comparisons = 0;
            swaps = 0;
            int n = arr.Length;
            for (int i = 1; i < n; i++)
            {
                int key = arr[i];
                int j = i - 1;
                while (j >= 0)
                {
                    comparisons++;
                    if (arr[j] > key)
                    {
                        swaps++;
                        arr[j + 1] = arr[j];
                        j = j - 1;
                    }
                    else
                    {
                        break;
                    }
                }
                arr[j + 1] = key;
            }
        }

        private static void MergeCSharp(int[] arr, int[] temp, int l, int m, int r, ref long comparisons, ref long swaps)
        {
            int i = l, j = m + 1, k = l;

            while (i <= m && j <= r)
            {
                comparisons++;
                if (arr[i] <= arr[j])
                {
                    swaps++;
                    temp[k++] = arr[i++];
                }
                else
                {
                    swaps++;
                    temp[k++] = arr[j++];
                }
            }

            while (i <= m)
            {
                swaps++;
                temp[k++] = arr[i++];
            }

            while (j <= r)
            {
                swaps++;
                temp[k++] = arr[j++];
            }

            for (i = l; i <= r; i++)
            {
                swaps++;
                arr[i] = temp[i];
            }
        }

        private static void MergeSortCSharp_Helper(int[] arr, int[] temp, int l, int r, ref long comparisons, ref long swaps)
        {
            if (l < r)
            {
                int m = l + (r - l) / 2;
                MergeSortCSharp_Helper(arr, temp, l, m, ref comparisons, ref swaps);
                MergeSortCSharp_Helper(arr, temp, m + 1, r, ref comparisons, ref swaps);
                MergeCSharp(arr, temp, l, m, r, ref comparisons, ref swaps);
            }
        }

        public static void MergeSortCSharp(int[] arr, out long comparisons, out long swaps)
        {
            comparisons = 0;
            swaps = 0;
            int[] temp = new int[arr.Length];
            MergeSortCSharp_Helper(arr, temp, 0, arr.Length - 1, ref comparisons, ref swaps);
        }

        private static void QuickSortCSharp_Helper(int[] arr, int low, int high, ref long comparisons, ref long swaps)
        {
            if (low < high)
            {
                int mid = low + (high - low) / 2;
                swaps++;
                int tempMid = arr[mid];
                arr[mid] = arr[high];
                arr[high] = tempMid;

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
        public static extern void SelectionSortC(int[] arr, int size, out long comparisons, out long swaps);

        [DllImport("SortingCore.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void InsertionSortC(int[] arr, int size, out long comparisons, out long swaps);

        [DllImport("SortingCore.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void MergeSortC(int[] arr, int size, out long comparisons, out long swaps);

        [DllImport("SortingCore.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void QuickSortC(int[] arr, int size, out long comparisons, out long swaps);

        [DllImport("SortingCore.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void BubbleSortCpp(int[] arr, int size, out long comparisons, out long swaps);

        [DllImport("SortingCore.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SelectionSortCpp(int[] arr, int size, out long comparisons, out long swaps);

        [DllImport("SortingCore.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void InsertionSortCpp(int[] arr, int size, out long comparisons, out long swaps);

        [DllImport("SortingCore.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void MergeSortCpp(int[] arr, int size, out long comparisons, out long swaps);

        [DllImport("SortingCore.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void QuickSortCpp(int[] arr, int size, out long comparisons, out long swaps);
    }
}
