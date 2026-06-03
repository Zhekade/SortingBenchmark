using System;

namespace SortingRunner
{
    public class ArrayGenerator
    {
        private static readonly Random _random = new Random();

        public static int[] GenerateRandom(int size)
        {
            int[] arr = new int[size];
            for (int i = 0; i < size; i++)
            {
                arr[i] = _random.Next(0, size * 10);
            }
            return arr;
        }

        public static int[] GenerateSorted(int size)
        {
            int[] arr = new int[size];
            for (int i = 0; i < size; i++)
            {
                arr[i] = i;
            }
            return arr;
        }

        public static int[] GenerateReversed(int size)
        {
            int[] arr = new int[size];
            for (int i = 0; i < size; i++)
            {
                arr[i] = size - i - 1;
            }
            return arr;
        }

        public static int[] CopyArray(int[] source)
        {
            int[] copy = new int[source.Length];
            Array.Copy(source, copy, source.Length);
            return copy;
        }
    }
}
