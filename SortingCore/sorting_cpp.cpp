#include "SortingCore.h"
#include <utility> // для std::swap

void BubbleSortCpp(int* arr, int size, long long* comparisons, long long* swaps) {
    *comparisons = 0;
    *swaps = 0;
    for (int i = 0; i < size - 1; ++i) {
        for (int j = 0; j < size - i - 1; ++j) {
            (*comparisons)++;
            if (arr[j] > arr[j + 1]) {
                (*swaps)++;
                std::swap(arr[j], arr[j + 1]);
            }
        }
    }
}

void QuickSortCpp_Helper(int* arr, int low, int high, long long* comparisons, long long* swaps) {
    if (low < high) {
        int mid = low + (high - low) / 2;
        (*swaps)++;
        std::swap(arr[mid], arr[high]);

        int pivot = arr[high];
        int i = (low - 1);

        for (int j = low; j <= high - 1; ++j) {
            (*comparisons)++;
            if (arr[j] < pivot) {
                i++;
                if (i != j) {
                    (*swaps)++;
                    std::swap(arr[i], arr[j]);
                }
            }
        }
        
        i++;
        if (i != high) {
            (*swaps)++;
            std::swap(arr[i], arr[high]);
        }

        int pi = i;

        QuickSortCpp_Helper(arr, low, pi - 1, comparisons, swaps);
        QuickSortCpp_Helper(arr, pi + 1, high, comparisons, swaps);
    }
}

void QuickSortCpp(int* arr, int size, long long* comparisons, long long* swaps) {
    *comparisons = 0;
    *swaps = 0;
    QuickSortCpp_Helper(arr, 0, size - 1, comparisons, swaps);
}
