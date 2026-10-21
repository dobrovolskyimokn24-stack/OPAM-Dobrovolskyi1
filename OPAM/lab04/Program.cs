using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lab04
{
    internal class Program
    {
        private const int N = 7;
        private const int K = 17;

        static void Main(string[] args)
        {
            Console.WriteLine($"--- Параметри варіанта ---");
            Console.WriteLine($"N = {N}, K = {K}, a = {N % 3}, b = {N % 4}, c = {N % 5}\n");

            List<int> mainList = GenerateMainList(N, 20);
            
            Console.Write("Основний список: ");
            PrintList(mainList);
            Console.WriteLine($"Кількість елементів: {mainList.Count}\n");

            // Завдання 1
            Console.WriteLine("=== Завдання 1 · Побудова (елементи, кратні 3) ===");
            List<int> task1Result = FilterMultiplesOfThree(mainList);
            Console.Write("Результат: ");
            PrintList(task1Result);
            Console.WriteLine($"Count: {task1Result.Count}\n");

            // Завдання 2
            Console.WriteLine("=== Завдання 2 · Вилучення від'ємних елементів ===");
            List<int> listWrong = CopyList(mainList);
            RemoveNegativesWrong(listWrong);
            Console.Write("1. Хибний варіант (з початку): ");
            PrintList(listWrong);

            List<int> listFromEnd = CopyList(mainList);
            RemoveNegativesFromEnd(listFromEnd);
            Console.Write("2. Правильний (з кінця):     ");
            PrintList(listFromEnd);

            List<int> listNew = RemoveNegativesNewList(mainList);
            Console.Write("3. Правильний (новий список): ");
            PrintList(listNew);

            bool isTask2Equal = AreListsEqual(listFromEnd, listNew);
            Console.WriteLine($"Результати варіантів 2 і 3 збігаються: {isTask2Equal}\n");

            // Завдання 3
            Console.WriteLine("=== Завдання 3 · Злиття впорядкованих списків ===");
            int len1 = 10 + K % 5;
            int len2 = 6 + K % 4;
            
            List<int> list1 = GenerateSortedList(N, len1);
            List<int> list2 = GenerateSortedList(N + 100, len2);

            Console.Write("Список 1: ");
            PrintList(list1);
            Console.Write("Список 2: ");
            PrintList(list2);

            List<int> mergedList = MergeSortedLists(list1, list2);
            Console.Write("Злитий список: ");
            PrintList(mergedList);

            bool isSorted = CheckIsSorted(mergedList);
            Console.WriteLine($"Список впорядкований: {isSorted}\n");

            // Завдання 4
            Console.WriteLine("=== Завдання 4 · Розділення на парні/непарні індекси ===");
            List<int> evenIndices;
            List<int> oddIndices;
            
            SplitByIndices(mainList, out evenIndices, out oddIndices);
            
            Console.Write("Парні індекси:   ");
            PrintList(evenIndices);
            Console.Write("Непарні індекси: ");
            PrintList(oddIndices);

            bool isCountCorrect = (evenIndices.Count + oddIndices.Count) == mainList.Count;
            Console.WriteLine($"Сума Count збігається з вихідним: {isCountCorrect}\n");

            // Завдання 5
            Console.WriteLine("=== Завдання 5 · Замір часу ===");
            RunBenchmark();
        }

        static List<int> GenerateMainList(int seed, int count)
        {
            Random rnd = new Random(seed);
            List<int> list = new List<int>(count);
            for (int i = 0; i < count; i++)
            {
                list.Add(rnd.Next(-9, 10));
            }
            return list;
        }

        static List<int> CopyList(List<int> source)
        {
            List<int> copy = new List<int>(source.Count);
            for (int i = 0; i < source.Count; i++)
            {
                copy.Add(source[i]);
            }
            return copy;
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

        static bool AreListsEqual(List<int> a, List<int> b)
        {
            if (a.Count != b.Count) return false;
            for (int i = 0; i < a.Count; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }

        static List<int> FilterMultiplesOfThree(List<int> source)
        {
            List<int> result = new List<int>();
            for (int i = 0; i < source.Count; i++)
            {
                if (source[i] % 3 == 0)
                {
                    result.Add(source[i]);
                }
            }
            return result;
        }

        static void RemoveNegativesWrong(List<int> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] < 0)
                {
                    list.RemoveAt(i);
                }
            }
        }

        static void RemoveNegativesFromEnd(List<int> list)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i] < 0)
                {
                    list.RemoveAt(i);
                }
            }
        }

        static List<int> RemoveNegativesNewList(List<int> source)
        {
            List<int> result = new List<int>();
            for (int i = 0; i < source.Count; i++)
            {
                if (source[i] >= 0)
                {
                    result.Add(source[i]);
                }
            }
            return result;
        }

        static List<int> GenerateSortedList(int seed, int length)
        {
            Random rnd = new Random(seed);
            List<int> list = new List<int>(length);
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

        static bool CheckIsSorted(List<int> list)
        {
            for (int i = 0; i < list.Count - 1; i++)
            {
                if (list[i] > list[i + 1]) return false;
            }
            return true;
        }

        static void SplitByIndices(List<int> source, out List<int> evenList, out List<int> oddList)
        {
            evenList = new List<int>((source.Count + 1) / 2);
            oddList = new List<int>(source.Count / 2);

            for (int i = 0; i < source.Count; i++)
            {
                if (i % 2 == 0)
                {
                    evenList.Add(source[i]);
                }
                else
                {
                    oddList.Add(source[i]);
                }
            }
        }

        static void RunBenchmark()
        {
            int elementsCount = 100000;
            int containsCount = 1000;

            List<int> warmup = new List<int>();
            for (int i = 0; i < 1000; i++) warmup.Add(i);
            warmup.Insert(0, -1);
            warmup.Contains(-99);

            Stopwatch sw = new Stopwatch();

            List<int> list1 = new List<int>();
            sw.Start();
            for (int i = 0; i < elementsCount; i++) list1.Add(i);
            sw.Stop();
            long t1 = sw.ElapsedMilliseconds;

            List<int> list2 = new List<int>(elementsCount);
            sw.Restart();
            for (int i = 0; i < elementsCount; i++) list2.Add(i);
            sw.Stop();
            long t2 = sw.ElapsedMilliseconds;

            List<int> list3 = new List<int>();
            sw.Restart();
            for (