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

void SelectionSortC(int* arr, int size, long long* comparisons, long long* swaps) {
    *comparisons = 0;
    *swaps = 0;
    for (int i = 0; i < size - 1; i++) {
        int min_idx = i;
        for (int j = i + 1; j < size; j++) {
            (*comparisons)++;
            if (arr[j] < arr[min_idx]) {
                min_idx = j;
            }
        }
        if (min_idx != i) {
            (*swaps)++;
            int temp = arr[i];
            arr[i] = arr[min_idx];
            arr[min_idx] = temp;
        }
    }
}

void InsertionSortC(int* arr, int size, long long* comparisons, long long* swaps) {
    *comparisons = 0;
    *swaps = 0;
    for (int i = 1; i < size; i++) {
        int key = arr[i];
        int j = i - 1;
        while (j >= 0) {
            (*comparisons)++;
            if (arr[j] > key) {
                (*swaps)++;
                arr[j + 1] = arr[j];
                j = j - 1;
            } else {
                break;
            }
        }
        arr[j + 1] = key;
    }
}

#include <stdlib.h>

void MergeC(int* arr, int* temp, int l, int m, int r, long long* comparisons, long long* swaps) {
    int i = l, j = m + 1, k = l;

    while (i <= m && j <= r) {
        (*comparisons)++;
        if (arr[i] <= arr[j]) {
            (*swaps)++;
            temp[k++] = arr[i++];
        } else {
            (*swaps)++;
            temp[k++] = arr[j++];
        }
    }

    while (i <= m) {
        (*swaps)++;
        temp[k++] = arr[i++];
    }

    while (j <= r) {
        (*swaps)++;
        temp[k++] = arr[j++];
    }

    for (i = l; i <= r; i++) {
        (*swaps)++;
        arr[i] = temp[i];
    }
}

void MergeSortC_Helper(int* arr, int* temp, int l, int r, long long* comparisons, long long* swaps) {
    if (l < r) {
        int m = l + (r - l) / 2;
        MergeSortC_Helper(arr, temp, l, m, comparisons, swaps);
        MergeSortC_Helper(arr, temp, m + 1, r, comparisons, swaps);
        MergeC(arr, temp, l, m, r, comparisons, swaps);
    }
}

void MergeSortC(int* arr, int size, long long* comparisons, long long* swaps) {
    *comparisons = 0;
    *swaps = 0;
    int* temp = (int*)malloc(size * sizeof(int));
    MergeSortC_Helper(arr, temp, 0, size - 1, comparisons, swaps);
    free(temp);
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
