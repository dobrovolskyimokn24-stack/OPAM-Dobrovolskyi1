using System;

public static class Sorts
{
    // Завдання 1: Сортування вставками
    public static void InsertionSort<T>(T[] a, Func<T, T, int> cmp, out int comparisons, out int moves)
    {
        comparisons = 0;
        moves = 0;
        for (int i = 1; i < a.Length; i++)
        {
            T key = a[i];
            moves++; // Запис key у тимчасову змінну
            int j = i - 1;

            while (j >= 0)
            {
                comparisons++;
                if (cmp(a[j], key) > 0)
                {
                    a[j + 1] = a[j];
                    moves++; // Зсув елемента
                    j--;
                }
                else
                {
                    break;
                }
            }
            a[j + 1] = key;
            moves++; // Вставка key у позицію
        }
    }

    // Завдання 2: Швидке сортування з опорним element - медіана трьох
    public static void QuickSortMedian3<T>(T[] a, Func<T, T, int> cmp, out int comparisons, out int moves)
    {
        int compCounter = 0;
        int moveCounter = 0;

        QuickSortInternal(a, 0, a.Length - 1, cmp, ref compCounter, ref moveCounter);

        comparisons = compCounter;
        moves = moveCounter;
    }

    private static void QuickSortInternal<T>(T[] a, int low, int high, Func<T, T, int> cmp, ref int compCounter, ref int moveCounter)
    {
        if (low < high)
        {
            int pivotIndex = Partition(a, low, high, cmp, ref compCounter, ref moveCounter);
            QuickSortInternal(a, low, pivotIndex - 1, cmp, ref compCounter, ref moveCounter);
            QuickSortInternal(a, pivotIndex + 1, high, cmp, ref compCounter, ref moveCounter);
        }
    }

    private static int MedianOfThree<T>(T[] a, int low, int high, Func<T, T, int> cmp, ref int compCounter, ref int moveCounter)
    {
        int mid = low + (high - low) / 2;

        // Пошук медіани серед a[low], a[mid], a[high]
        if (CompareAndSwap(a, low, mid, cmp, ref compCounter, ref moveCounter)) { }
        if (CompareAndSwap(a, low, high, cmp, ref compCounter, ref moveCounter)) { }
        if (CompareAndSwap(a, mid, high, cmp, ref compCounter, ref moveCounter)) { }

        // Міняємо медіану (на mid) з передостаннім елементом high - 1 (класична схема)
        Swap(a, mid, high - 1, ref moveCounter);
        return high - 1;
    }

    private static bool CompareAndSwap<T>(T[] a, int i, int j, Func<T, T, int> cmp, ref int compCounter, ref int moveCounter)
    {
        compCounter++;
        if (cmp(a[i], a[j]) > 0)
        {
            Swap(a, i, j, ref moveCounter);
            return true;
        }
        return false;
    }

    private static int Partition<T>(T[] a, int low, int high, Func<T, T, int> cmp, ref int compCounter, ref int moveCounter)
    {
        if (high - low >= 2)
        {
            int pivotIdx = MedianOfThree(a, low, high, cmp, ref compCounter, ref moveCounter);
            T pivot = a[pivotIdx];
            int i = low;
            int j = high - 1;

            while (true)
            {
                while (true)
                {
                    compCounter++;
                    if (cmp(a[++i], pivot) >= 0) break;
                }
                while (true)
                {
                    compCounter++;
                    if (cmp(a[--j], pivot) <= 0) break;
                }
                if (i >= j) break;
                Swap(a, i, j, ref moveCounter);
            }
            Swap(a, i, high - 1, ref moveCounter);
            return i;
        }
        else
        {
            compCounter++;
            if (cmp(a[low], a[high]) > 0)
            {
                Swap(a, low, high, ref moveCounter);
            }
            return low;
        }
    }

    private static void Swap<T>(T[] a, int i, int j, ref int moveCounter)
    {
        T temp = a[i];
        a[i] = a[j];
        a[j] = temp;
        moveCounter += 3;
    }

    // Завдання 3: Гібридне сортування (QuickSort + InsertionSort для підмасивів <= threshold)
    public static void HybridSort<T>(T[] a, int threshold, Func<T, T, int> cmp, out int comparisons, out int moves)
    {
        int compCounter = 0;
        int moveCounter = 0;

        HybridSortInternal(a, 0, a.Length - 1, threshold, cmp, ref compCounter, ref moveCounter);

        comparisons = compCounter;
        moves = moveCounter;
    }

    private static void HybridSortInternal<T>(T[] a, int low, int high, int threshold, Func<T, T, int> cmp, ref int compCounter, ref int moveCounter)
    {
        if (high - low + 1 <= threshold)
        {
            InsertionSortSubarray(a, low, high, cmp, ref compCounter, ref moveCounter);
        }
        else if (low < high)
        {
            int pivotIndex = Partition(a, low, high, cmp, ref compCounter, ref moveCounter);
            HybridSortInternal(a, low, pivotIndex - 1, threshold, cmp, ref compCounter, ref moveCounter);
            HybridSortInternal(a, pivotIndex + 1, high, threshold, cmp, ref compCounter, ref moveCounter);
        }
    }

    private static void InsertionSortSubarray<T>(T[] a, int low, int high, Func<T, T, int> cmp, ref int compCounter, ref int moveCounter)
    {
        for (int i = low + 1; i <= high; i++)
        {
            T key = a[i];
            moveCounter++;
            int j = i - 1;

            while (j >= low)
            {
                compCounter++;
                if (cmp(a[j], key) > 0)
                {
                    a[j + 1] = a[j];
                    moveCounter++;
                    j--;
                }
                else
                {
                    break;
                }
            }
            a[j + 1] = key;
            moveCounter++;
        }
    }
}