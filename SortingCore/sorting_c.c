#include "SortingCore.h"

void BubbleSortC(int* arr, int size, long long* comparisons, long long* swaps) {
    *comparisons = 0;
    *swaps = 0;
    for (int i = 0; i < size - 1; i++) {
        for (int j = 0; j < size - i - 1; j++) {
            (*comparisons)++;
            if (arr[j] > arr[j + 1]) {
                (*swaps)++;
                int temp = arr[j];
                arr[j] = arr[j + 1];
                arr[j + 1] = temp;
            }
        }
    }
}

void QuickSortC_Helper(int* arr, int low, int high, long long* comparisons, long long* swaps) {
    if (low < high) {
        int mid = low + (high - low) / 2;
        (*swaps)++;
        int tempMid = arr[mid];
        arr[mid] = arr[high];
        arr[high] = tempMid;

        int pivot = arr[high];
        int i = (low - 1);

        for (int j = low; j <= high - 1; j++) {
            (*comparisons)++;
            if (arr[j] < pivot) {
                i++;
                if (i != j) {
                    (*swaps)++;
                    int temp = arr[i];
                    arr[i] = arr[j];
                    arr[j] = temp;
                }
            }
        }
        
        i++;
        if (i != high) {
            (*swaps)++;
            int temp = arr[i];
            arr[i] = arr[high];
            arr[high] = temp;
        }

        int pi = i;

        QuickSortC_Helper(arr, low, pi - 1, comparisons, swaps);
        QuickSortC_Helper(arr, pi + 1, high, comparisons, swaps);
    }
}

void QuickSortC(int* arr, int size, long long* comparisons, long long* swaps) {
    *comparisons = 0;
    *swaps = 0;
    QuickSortC_Helper(arr, 0, size - 1, comparisons, swaps);
}
