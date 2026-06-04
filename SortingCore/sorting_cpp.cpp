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

void SelectionSortCpp(int* arr, int size, long long* comparisons, long long* swaps) {
    *comparisons = 0;
    *swaps = 0;
    for (int i = 0; i < size - 1; ++i) {
        int min_idx = i;
        for (int j = i + 1; j < size; ++j) {
            (*comparisons)++;
            if (arr[j] < arr[min_idx]) {
                min_idx = j;
            }
        }
        if (min_idx != i) {
            (*swaps)++;
            std::swap(arr[i], arr[min_idx]);
        }
    }
}

void InsertionSortCpp(int* arr, int size, long long* comparisons, long long* swaps) {
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



void MergeCpp(int* arr, int* temp, int l, int m, int r, long long* comparisons, long long* swaps) {
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

void MergeSortCpp_Helper(int* arr, int* temp, int l, int r, long long* comparisons, long long* swaps) {
    if (l < r) {
        int m = l + (r - l) / 2;
        MergeSortCpp_Helper(arr, temp, l, m, comparisons, swaps);
        MergeSortCpp_Helper(arr, temp, m + 1, r, comparisons, swaps);
        MergeCpp(arr, temp, l, m, r, comparisons, swaps);
    }
}

void MergeSortCpp(int* arr, int size, long long* comparisons, long long* swaps) {
    *comparisons = 0;
    *swaps = 0;
    int* temp = new int[size];
    MergeSortCpp_Helper(arr, temp, 0, size - 1, comparisons, swaps);
    delete[] temp;
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
