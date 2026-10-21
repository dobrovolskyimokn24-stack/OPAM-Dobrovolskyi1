using System;
using System.Collections.Generic;
using System.IO;

namespace Lab03
{
    internal class Program
    {
        private const int N = 7;
        private const int K = 17;

        static void Main(string[] args)
        {
            Console.WriteLine($"--- Параметри варіанта ---");
            Console.WriteLine($"N = {N}, K = {K}, a = {N % 3}, b = {N % 4}, c = {N % 5}\n");

            // Завдання 1
            Console.WriteLine("=== Завдання 1 · Обхід каталогів (.cs файли) ===");
            Console.Write("Введіть шлях до папки (Enter для поточної): ");
            string inputPath = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(inputPath))
            {
                inputPath = Directory.GetCurrentDirectory();
            }

            if (!Directory.Exists(inputPath))
            {
                Console.WriteLine("Вказана папка не існує!");
            }
            else
            {
                int csCount = 0;
                long csTotalSize = 0;
                CountCsFilesAndSize(inputPath, ref csCount, ref csTotalSize);
                Console.WriteLine($"Знайдено .cs файлів: {csCount}");
                Console.WriteLine($"Сумарний розмір: {csTotalSize} байт\n");
            }

            // Завдання 2
            Console.WriteLine("=== Завдання 2 · Розбиття навпіл (числа > 7) ===");
            int threshold = K % 10;
            int[] array16 = GenerateArray(16, -20, 20, N);
            Console.Write("Масив [16]: ");
            PrintArray(array16);

            int maxDepth = 0;
            int countRec = SolveDivideAndConquer(array16, 0, array16.Length - 1, threshold, 1, ref maxDepth);
            Console.WriteLine($"Кількість елементів > {threshold}: {countRec}");
            Console.WriteLine($"Максимальна глибина рекурсії: {maxDepth} (log2(16) = 4)\n");

            // Завдання 3
            Console.WriteLine("=== Завдання 3 · Перебір способів піднятися на 6 сходинок ===");
            int stepsCount = K % 8 + 5;
            List<int> currentPath = new List<int>();
            int totalWays = 0;
            ClimbStairs(stepsCount, currentPath, ref totalWays);
            Console.WriteLine($"Усього способів: {totalWays}\n");

            // Завдання 4
            Console.WriteLine("=== Завдання 4 · Трасування масиву з 4 елементів ===");
            int[] array4 = new int[] { array16[0], array16[1], array16[2], array16[3] };
            Console.Write("Масив для трасування: ");
            PrintArray(array4);
            TraceSolve(array4, 0, array4.Length - 1, threshold, 0);
            Console.WriteLine();

            // Завдання 5
            Console.WriteLine("=== Завдання 5 · Перевірка ітеративної версії ===");
            int countIter = SolveIterative(array16, threshold);
            Console.WriteLine($"Рекурсивно: {countRec}");
            Console.WriteLine($"Ітеративно: {countIter}");
            Console.WriteLine($"Результати збігаються: {countRec == countIter}\n");
        }

        static int[] GenerateArray(int length, int minVal, int maxVal, int seed)
        {
            Random rnd = new Random(seed);
            int[] arr = new int[length];
            for (int i = 0; i < length; i++)
            {
                arr[i] = rnd.Next(minVal, maxVal + 1);
            }
            return arr;
        }

        static void PrintArray(int[] arr)
        {
            Console.Write("[");
            for (int i = 0; i < arr.Length; i++)
            {
                Console.Write(arr[i] + (i < arr.Length - 1 ? ", " : ""));
            }
            Console.WriteLine("]");
        }

        static void CountCsFilesAndSize(string dirPath, ref int csCount, ref long totalSize)
        {
            try
            {
                string[] files = Directory.GetFiles(dirPath);
                foreach (string f in files)
                {
                    if (f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                    {
                        csCount++;
                        FileInfo fi = new FileInfo(f);
                        totalSize += fi.Length;
                    }
                }

                string[] subDirs = Directory.GetDirectories(dirPath);
                foreach (string d in subDirs)
                {
                    CountCsFilesAndSize(d, ref csCount, ref totalSize);
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (DirectoryNotFoundException) { }
        }

        static int SolveDivideAndConquer(int[] arr, int lo, int hi, int threshold, int currentDepth, ref int maxDepth)
        {
            if (currentDepth > maxDepth) maxDepth = currentDepth;

            if (lo == hi)
            {
                return arr[lo] > threshold ? 1 : 0;
            }

            int mid = (lo + hi) / 2;
            int leftCount = SolveDivideAndConquer(arr, lo, mid, threshold, currentDepth + 1, ref maxDepth);
            int rightCount = SolveDivideAndConquer(arr, mid + 1, hi, threshold, currentDepth + 1, ref maxDepth);

            return leftCount + rightCount;
        }

        static void ClimbStairs(int remainingSteps, List<int> path, ref int waysCount)
        {
            if (remainingSteps == 0)
            {
                waysCount++;
                Console.WriteLine($"Варіант {waysCount}: " + string.Join(" + ", path));
                return;
            }

            for (int step = 1; step <= 2; step++)
            {
                if (remainingSteps - step >= 0)
                {
                    path.Add(step);
                    ClimbStairs(remainingSteps - step, path, ref waysCount);
                    path.RemoveAt(path.Count - 1);
                }
            }
        }

        static int TraceSolve(int[] arr, int lo, int hi, int threshold, int depth)
        {
            string indent = new string(' ', depth * 2);
            Console.WriteLine($"{indent}enter Count({lo}..{hi})");

            if (lo == hi)
            {
                int val = arr[lo] > threshold ? 1 : 0;
                Console.WriteLine($"{indent}  base Count({lo}..{hi}) -> {val}");
                return val;
            }

            int mid = (lo + hi) / 2;
            int left = TraceSolve(arr, lo, mid, threshold, depth + 1);
            int right = TraceSolve(arr, mid + 1, hi, threshold, depth + 1);
            int res = left + right;

            Console.WriteLine($"{indent}exit  Count({lo}..{hi}) -> {res}");
            return res;
        }

        static int SolveIterative(int[] arr, int threshold)
        {
            int count = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] > threshold) count++;
            }
            return count;
        }
    }
}