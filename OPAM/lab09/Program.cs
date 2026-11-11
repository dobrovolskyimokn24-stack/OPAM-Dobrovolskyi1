using System;
using System.Diagnostics;
using System.Linq;

class Program
{
    delegate void SortDelegate(int[] arr, out int comp, out int moves);

    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        int N = 7;
        Func<int, int, int> cmp = (x, y) => x.CompareTo(y);

        Console.WriteLine($"=== ЛАБОРАТОРНА РОБОТА 9 | N={N} ===\n");

        // --- Завдання 1: Трасування перших 8 елементів ---
        Console.WriteLine("--- Завдання 1: Трасування InsertionSort (перші 8 елементів) ---");
        int[] rawData = GenerateData(100000, N);
        int[] traceArray = rawData.Take(8).ToArray();
        Console.WriteLine("Початковий стан: " + string.Join(", ", traceArray));

        for (int i = 1; i < traceArray.Length; i++)
        {
            int key = traceArray[i];
            int j = i - 1;
            while (j >= 0 && traceArray[j] > key)
            {
                traceArray[j + 1] = traceArray[j];
                j--;
            }
            traceArray[j + 1] = key;
            Console.WriteLine($"Прохід {i} (вставка {key}): " + string.Join(", ", traceArray));
        }
        Console.WriteLine();

        // --- Завдання 4: Таблиця порівнянь та переставлянь ---
        Console.WriteLine("--- Завдання 4: Лічильники (Порівняння / Переставляння) ---");
        int[] sizes = { 100, 1000, 10000 };

        SortDelegate insertSortDel = (int[] arr, out int c, out int m) => Sorts.InsertionSort(arr, cmp, out c, out m);
        SortDelegate quickSortDel = (int[] arr, out int c, out int m) => Sorts.QuickSortMedian3(arr, cmp, out c, out m);

        foreach (int size in sizes)
        {
            Console.WriteLine($"\n[Розмір масиву: {size}]");
            var arrays = CreateFourInputs(size, N);

            RunTest("InsertionSort", arrays, insertSortDel);
            RunTest("QuickSort (Med3)", arrays, quickSortDel);
        }

        // --- Завдання 3: Порівняння гібридного сортування з різними порогами ---
        Console.WriteLine("\n--- Завдання 3: Гібридне сортування (Пороги 1, 4, 16, 37) на n=100 000 ---");
        int[] thresholds = { 1, 4, 16, 37 };
        int[] testArr = GenerateData(100000, N);

        foreach (int t in thresholds)
        {
            long time = MeasureTime(() =>
            {
                int[] copy = (int[])testArr.Clone();
                Sorts.HybridSort(copy, t, cmp, out _, out _);
            });
            Console.WriteLine($"Поріг {t,2}: {time} мс");
        }

        // --- Завдання 5: Часовий тест проти Array.Sort ---
        Console.WriteLine("\n--- Завдання 5: Вимірювання часу проти Array.Sort (мс) ---");
        int[] timeSizes = { 1000, 10000, 100000 };
        Console.WriteLine("n\t| InsertionSort | QuickSort | Hybrid (16) | Array.Sort");
        Console.WriteLine("---------------------------------------------------------");

        foreach (int s in timeSizes)
        {
            int[] data = GenerateData(s, N);

            long t1 = s <= 10000 ? MeasureTime(() => Sorts.InsertionSort((int[])data.Clone(), cmp, out _, out _)) : -1;
            long t2 = MeasureTime(() => Sorts.QuickSortMedian3((int[])data.Clone(), cmp, out _, out _));
            long t3 = MeasureTime(() => Sorts.HybridSort((int[])data.Clone(), 16, cmp, out _, out _));
            long t4 = MeasureTime(() => { int[] copy = (int[])data.Clone(); Array.Sort(copy); });

            string t1Str = t1 == -1 ? ">1000" : t1.ToString();
            Console.WriteLine($"{s}\t| {t1Str,-13} | {t2,-9} | {t3,-11} | {t4}");
        }

        // Межа прийнятності для InsertionSort
        Console.WriteLine("\nПошук межі прийнятності для InsertionSort (поки не перевищить 1 сек):");
        int nAccept = 10000;
        while (true)
        {
            int[] d = GenerateData(nAccept, N);
            long t = MeasureTime(() => Sorts.InsertionSort((int[])d.Clone(), cmp, out _, out _));
            if (t > 1000)
            {
                Console.WriteLine($"InsertionSort перевищує 1 секунду при n = {nAccept} (час: {t} мс)");
                break;
            }
            nAccept += 5000;
        }

        TestStability();
        TestEdgeCases(cmp);
    }

    static int[] GenerateData(int size, int seed)
    {
        Random rnd = new Random(seed);
        int[] data = new int[size];
        for (int i = 0; i < size; i++) data[i] = rnd.Next(0, 341);
        return data;
    }

    static (int[] rnd, int[] sorted, int[] rev, int[] almost) CreateFourInputs(int size, int seed)
    {
        int[] r = GenerateData(size, seed);
        int[] s = (int[])r.Clone();
        Array.Sort(s);
        int[] rev = (int[])s.Clone();
        Array.Reverse(rev);

        int[] alm = (int[])s.Clone();
        Random rnd = new Random(seed);
        for (int i = 0; i < 17; i++)
        {
            int idx1 = rnd.Next(size);
            int idx2 = rnd.Next(size);
            int tmp = alm[idx1]; alm[idx1] = alm[idx2]; alm[idx2] = tmp;
        }

        return (r, s, rev, alm);
    }

    static void RunTest(string name, (int[] rnd, int[] sorted, int[] rev, int[] almost) inputs, SortDelegate sorter)
    {
        Console.WriteLine($"Algorithm: {name}");
        TestOne(inputs.rnd, "Випадковий", sorter);
        TestOne(inputs.sorted, "Відсортований", sorter);
        TestOne(inputs.rev, "Навпаки", sorter);
        TestOne(inputs.almost, "Майже", sorter);
    }

    static void TestOne(int[] source, string inputType, SortDelegate sorter)
    {
        int[] copy = (int[])source.Clone();
        sorter(copy, out int c, out int m);
        bool isSorted = Verification.IsSorted(copy, (x, y) => x.CompareTo(y));
        bool isPerm = Verification.IsPermutationOf(source, copy);
        Console.WriteLine($"  {inputType,-13}: Cmp = {c,8}, Moves = {m,8} | Correct: {isSorted && isPerm}");
    }

    static long MeasureTime(Action act)
    {
        act();
        long minTime = long.MaxValue;
        for (int i = 0; i < 3; i++)
        {
            var sw = Stopwatch.StartNew();
            act();
            sw.Stop();
            if (sw.ElapsedMilliseconds < minTime) minTime = sw.ElapsedMilliseconds;
        }
        return minTime;
    }

    static void TestStability()
    {
        Console.WriteLine("\n--- Перевірка стабільності (Student[]) ---");
        Student[] students = {
            new Student("Анатолій", "КН-3", 90),
            new Student("Богдан", "КН-3", 90),
            new Student("Василь", "КН-3", 90)
        };

        Func<Student, Student, int> cmpScore = (s1, s2) => s1.Score.CompareTo(s2.Score);

        Student[] copy1 = (Student[])students.Clone();
        Sorts.InsertionSort(copy1, cmpScore, out int _, out int _);
        Console.WriteLine($"InsertionSort  (Очікується: Стабільний): Перший={copy1[0].Name} (Стабільний: {copy1[0].Name == "Анатолій"})");

        Student[] copy2 = (Student[])students.Clone();
        Sorts.QuickSortMedian3(copy2, cmpScore, out int _, out int _);
        Console.WriteLine($"QuickSortMed3  (Очікується: Нестабільний): Перший={copy2[0].Name}");
    }

    static void TestEdgeCases(Func<int, int, int> cmp)
    {
        Console.WriteLine("\n--- Крайові випадки ---");
        int[][] edgeCases = {
            new int[0],
            new int[] { 5 },
            new int[] { 1, 2 },
            new int[] { 2, 1 },
            new int[] { 3, 3, 3, 3 },
            new int[] { 1, 2, 1, 2, 1, 2 }
        };

        foreach (var arr in edgeCases)
        {
            int[] c1 = (int[])arr.Clone();
            Sorts.QuickSortMedian3(c1, cmp, out int _, out int _);
            bool ok = Verification.IsSorted(c1, cmp) && Verification.IsPermutationOf(arr, c1);
            Console.WriteLine($"Array size {arr.Length}: OK={ok}");
        }
    }
}
