#pragma once

// Експорт функцій для P/Invoke. Використовуємо extern "C" для уникнення Name Mangling.
#ifdef __cplusplus
extern "C" {
#endif

	// C-реалізації
	__declspec(dllexport) void BubbleSortC(int* arr, int size, long long* comparisons, long long* swaps);
	__declspec(dllexport) void QuickSortC(int* arr, int size, long long* comparisons, long long* swaps);

	// C++-реалізації
	__declspec(dllexport) void BubbleSortCpp(int* arr, int size, long long* comparisons, long long* swaps);
	__declspec(dllexport) void QuickSortCpp(int* arr, int size, long long* comparisons, long long* swaps);

#ifdef __cplusplus
}
#endif
