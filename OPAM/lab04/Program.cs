using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lab04
{
    internal class Program
    {
        private const int N = 7;
        private const int K = 8;

        static void Main(string[] args)
        {
            int a = N % 3;
            int b = N % 4;
            int c = N % 5;

            Console.WriteLine($"--- Параметри варіанта ---");
            Console.WriteLine($"N = {N}, K = {K}, a = {a}, b = {b}, c = {c}\n");

            List<int> mainList = GenerateMainList(N, 20);

            Console.Write("Основний список: ");
            PrintList(mainList);
            Console.WriteLine();

            Console.WriteLine("=== Завдання 1: Побудова за критерієм ===");
            List<int> task1Result = FilterByCriterion(mainList, a, K);
            Console.Write("Результат Завдання 1: ");
            PrintList(task1Result);
            Console.WriteLine($"Count: {task1Result.Count}\n");

            Console.WriteLine("=== Завдання 2: Вилучення (3 варіанти) ===");

            List<int> copy1 = new List<int>(mainList);
            RemoveIncorrectWay(copy1, c);
            Console.Write("1. Хибний варіант (з початку): ");
            PrintList(copy1);

            List<int> copy2 = new List<int>(mainList);
            RemoveFromEndWay(copy2, c);
            Console.Write("2. Правильний (з кінця):        ");
            PrintList(copy2);

            List<int> copy3 = RemoveNewListWay(mainList, c);
            Console.Write("3. Правильний (новий список):   ");
            PrintList(copy3);

            bool isTask2Equal = AreListsEqual(copy2, copy3);
            Console.WriteLine($"Варіанти 2 і 3 збігаються: {isTask2Equal}\n");

            Console.WriteLine("=== Завдання 3: Злиття двох упорядкованих списків ===");
            int len1 = 10 + K % 5;
            int len2 = 6 + K % 4;

            List<int> sortedList1 = GenerateSortedList(N, len1);
            List<int> sortedList2 = GenerateSortedList(N + 100, len2);

            Console.Write("Список 1: "); PrintList(sortedList1);
            Console.Write("Список 2: "); PrintList(sortedList2);

            List<int> mergedList = MergeSortedLists(sortedList1, sortedList2);
            Console.Write("Злитий:   "); PrintList(mergedList);

            bool isSortedCorrectly = IsSorted(mergedList);
            Console.WriteLine($"Список впорядкований коректно: {isSortedCorrectly}\n");

            Console.WriteLine("=== Завдання 4: Розділення списку на два ===");
            SplitList(mainList, b, K, out List<int> part1, out List<int> part2);

            Console.Write("Частина 1: "); PrintList(part1);
            Console.Write("Частина 2: "); PrintList(part2);

            bool isCountSumEqual = (part1.Count + part2.Count == mainList.Count);
            Console.WriteLine($"Сума Count обох частин дорівнює початкового ({mainList.Count}): {isCountSumEqual}\n");

            Console.WriteLine("=== Завдання 5: Замір часу (Stopwatch) ===");
            RunBenchmark();
        }

        static List<int> GenerateMainList(int seed, int size)
        {
            Random rnd = new Random(seed);
            List<int> list = new List<int>(size);
            for (int i = 0; i < size; i++)
            {
                list.Add(rnd.Next(-9, 10));
            }
            return list;
        }

        static void PrintList(List<int> list)
        {
            Console.Write("[");
            for (int i = 0; i < list.Count; i++)
            {
                Console.Write(list[i] + (i < list.Count - 1 ? ", " : ""));
            }
            Console.WriteLine("]");
        }

        static bool AreListsEqual(List<int> l1, List<int> l2)
        {
            if (l1.Count != l2.Count) return false;
            for (int i = 0; i < l1.Count; i++)
            {
                if (l1[i] != l2[i]) return false;
            }
            return true;
        }

        static List<int> FilterByCriterion(List<int> source, int a, int K)
        {
            List<int> result = new List<int>();

            for (int i = 0; i < source.Count; i++)
            {
                int item = source[i];
                bool keep = false;

                switch (a)
                {
                    case 0:
                        keep = item > (K % 7);
                        break;
                    case 1:
                        int divisor = (K % 4) + 2;
                        keep = (item % divisor == 0);
                        break;
                    case 2:
                        keep = (item > 0 && item % 2 != 0);
                        break;
                }

                if (keep)
                {
                    result.Add(item);
                }
            }

            return result;
        }

        static bool ShouldRemove(int item, int index, List<int> list, int c, int firstElemValue, double averageValue)
        {
            switch (c)
            {
                case 0: return item % 5 == 0;
                case 1: return item % 3 == 0;
                case 2: return item < 0;
                case 3: return item == firstElemValue;
                case 4: return item < averageValue;
                default: return false;
            }
        }

        static double CalculateAverage(List<int> list)
        {
            if (list.Count == 0) return 0;
            double sum = 0;
            for (int i = 0; i < list.Count; i++) sum += list[i];
            return sum / list.Count;
        }

        static void RemoveIncorrectWay(List<int> list, int c)
        {
            if (list.Count == 0) return;
            int firstElem = list[0];
            double avg = CalculateAverage(list);

            for (int i = 0; i < list.Count; i++)
            {
                if (ShouldRemove(list[i], i, list, c, firstElem, avg))
                {
                    list.RemoveAt(i);
                }
            }
        }

        static void RemoveFromEndWay(List<int> list, int c)
        {
            if (list.Count == 0) return;
            int firstElem = list[0];
            double avg = CalculateAverage(list);

            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (ShouldRemove(list[i], i, list, c, firstElem, avg))
                {
                    list.RemoveAt(i);
                }
            }
        }

        static List<int> RemoveNewListWay(List<int> list, int c)
        {
            List<int> result = new List<int>();
            if (list.Count == 0) return result;

            int firstElem = list[0];
            double avg = CalculateAverage(list);

            for (int i = 0; i < list.Count; i++)
            {
                if (!ShouldRemove(list[i], i, list, c, firstElem, avg))
                {
                    result.Add(list[i]);
                }
            }

            return result;
        }

        static List<int> GenerateSortedList(int seed, int length)
        {
            Random rnd = new Random(seed);
            List<int> list = new List<int>(length);
            if (length == 0) return list;

            int current = rnd.Next(1, 10);
            list.Add(current);

            for (int i = 1; i < length; i++)
            {
                current += rnd.Next(1, 10);
                list.Add(current);
            }

            return list;
        }

        static List<int> MergeSortedLists(List<int> list1, List<int> list2)
        {
            List<int> result = new List<int>(list1.Count + list2.Count);
            int i = 0, j = 0;

            while (i < list1.Count && j < list2.Count)
            {
                if (list1[i] <= list2[j])
                {
                    result.Add(list1[i]);
                    i++;
                }
                else
                {
                    result.Add(list2[j]);
                    j++;
                }
            }

            while (i < list1.Count)
            {
                result.Add(list1[i]);
                i++;
            }

            while (j < list2.Count)
            {
                result.Add(list2[j]);
                j++;
            }

            return result;
        }

        static bool IsSorted(List<int> list)
        {
            for (int i = 0; i < list.Count - 1; i++)
            {
                if (list[i] > list[i + 1]) return false;
            }
            return true;
        }

        static void SplitList(List<int> source, int b, int K, out List<int> part1, out List<int> part2)
        {
            part1 = new List<int>();
            part2 = new List<int>();

            for (int i = 0; i < source.Count; i++)
            {
                int item = source[i];
                bool goToPart1 = false;

                switch (b)
                {
                    case 0:
                        goToPart1 = (item < 0);
                        break;
                    case 1:
                        goToPart1 = (item % 2 == 0);
                        break;
                    case 2:
                        goToPart1 = (item < (K % 7));
                        break;
                    case 3:
                        goToPart1 = (i % 2 == 0);
                        break;
                }

                if (goToPart1)
                    part1.Add(item);
                else
                    part2.Add(item);
            }
        }

        static void RunBenchmark()
        {
            const int count = 100000;
            const int containsCount = 1000;

            List<int> warmup = new List<int>();
            for (int i = 0; i < 1000; i++) warmup.Add(i);
            warmup.Insert(0, -1);
            warmup.Contains(-999);

            Stopwatch sw = new Stopwatch();

            sw.Restart();
            List<int> list1 = new List<int>();
            for (int i = 0; i < count; i++) list1.Add(i);
            sw.Stop();
            long t1 = sw.ElapsedMilliseconds;

            sw.Restart();
            List<int> list2 = new List<int>(count);
            for (int i = 0; i < count; i++) list2.Add(i);
            sw.Stop();
            long t2 = sw.ElapsedMilliseconds;

            sw.Restart();
            List<int> list3 = new List<int>();
            for (int i = 0; i < count; i++) list3.Insert(0, i);
            sw.Stop();
            long t3 = sw.ElapsedMilliseconds;

            sw.Restart();
            for (int i = 0; i < containsCount; i++)
            {
                list1.Contains(-99999);
            }
            sw.Stop();
            long t4 = sw.ElapsedMilliseconds;

            Console.WriteLine($"| Операція | Час, мс |");
            Console.WriteLine($"| --- | --- |");
            Console.WriteLine($"| 100 000 × Add(x), без місткості | {t1} мс |");
            Console.WriteLine($"| 100 000 × Add(x), new List(100000) | {t2} мс |");
            Console.WriteLine($"| 100 000 × Insert(0, x) | {t3} мс |");
            Console.WriteLine($"| 1 000 × Contains(неіснуюче) у 100k списку | {t4} мс |");
        }
    }
}
