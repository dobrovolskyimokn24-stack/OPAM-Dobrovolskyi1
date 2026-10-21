using System;

namespace Lab02
{
    internal class Program
    {
        private const int N = 7;
        private const int K = 17;

        static void Main(string[] args)
        {
            Console.WriteLine($"N = {N}, K = {K}, a = {N % 3}, b = {N % 4}, c = {N % 5}\n");

            // Основний масив: 18 випадкових чисел у діапазоні [-3; 3], Random(7)
            int[] mainArray = GenerateArray(18, -3, 3, N);
            Console.Write("Основний масив: ");
            PrintArray(mainArray);
            Console.WriteLine();

            // Завдання 1
            Console.WriteLine("=== Завдання 1 · Агрегація ===");
            Task1_Aggregate(mainArray);
            Console.WriteLine();

            // Завдання 2
            Console.WriteLine("=== Завдання 2 · Фільтрація (кратні 3) ===");
            int[] filtered = Task2_FilterMultiplesOfThree(mainArray);
            Console.Write("Відфільтрований масив: ");
            PrintArray(filtered);
            Console.WriteLine($"Довжина: {filtered.Length}\n");

            // Завдання 3
            Console.WriteLine("=== Завдання 3 · Серії ===");
            Task3_LongestSeries(mainArray);
            Console.WriteLine();

            // Завдання 4
            Console.WriteLine("=== Завдання 4 · Перестановка на місці (циклічний зсув вправо) ===");
            int[] arrayForShift = CopyArray(mainArray);
            Console.Write("До:    ");
            PrintArray(arrayForShift);
            Task4_ShiftRight(arrayForShift);
            Console.Write("Після: ");
            PrintArray(arrayForShift);
            Console.WriteLine();

            // Завдання 5
            Console.WriteLine("=== Завдання 5 · Матриця 6x3 ===");
            int[,] matrix = GenerateMatrix(6, 3, -3, 3, N);
            Task5_ProcessMatrix(matrix);
            Console.WriteLine();

            // Завдання 6
            Console.WriteLine("=== Завдання 6 · Крайові випадки ===");
            RunEdgeCases();
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

        static int[] CopyArray(int[] source)
        {
            int[] copy = new int[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                copy[i] = source[i];
            }
            return copy;
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

        static void Task1_Aggregate(int[] arr)
        {
            if (arr == null || arr.Length == 0)
            {
                Console.WriteLine("Масив порожній, агрегація неможлива.");
                return;
            }

            int sum = 0;
            int minVal = arr[0];
            int maxVal = arr[0];
            int minIdx = 0;
            int maxIdx = 0;
            int zeroCount = 0;

            for (int i = 0; i < arr.Length; i++)
            {
                sum += arr[i];

                if (arr[i] < minVal)
                {
                    minVal = arr[i];
                    minIdx = i;
                }

                if (arr[i] > maxVal)
                {
                    maxVal = arr[i];
                    maxIdx = i;
                }

                if (arr[i] == 0)
                {
                    zeroCount++;
                }
            }

            double avg = (double)sum / arr.Length;

            Console.WriteLine($"Сума: {sum}");
            Console.WriteLine($"Середнє: {avg:F2}");
            Console.WriteLine($"Мінімум: {minVal} (індекс {minIdx})");
            Console.WriteLine($"Максимум: {maxVal} (індекс {maxIdx})");
            Console.WriteLine($"Кількість нулів: {zeroCount}");
        }

        static int[] Task2_FilterMultiplesOfThree(int[] arr)
        {
            if (arr == null || arr.Length == 0) return new int[0];

            int count = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] % 3 == 0)
                {
                    count++;
                }
            }

            int[] result = new int[count];
            int idx = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] % 3 == 0)
                {
                    result[idx++] = arr[i];
                }
            }

            return result;
        }

        static void Task3_LongestSeries(int[] arr)
        {
            if (arr == null || arr.Length == 0)
            {
                Console.WriteLine("Масив порожній, серії відсутні.");
                return;
            }

            int maxVal = arr[0];
            int maxLen = 1;
            int maxStartIdx = 0;

            int currentVal = arr[0];
            int currentLen = 1;
            int currentStartIdx = 0;

            for (int i = 1; i < arr.Length; i++)
            {
                if (arr[i] == currentVal)
                {
                    currentLen++;
                }
                else
                {
                    if (currentLen > maxLen)
                    {
                        maxLen = currentLen;
                        maxVal = currentVal;
                        maxStartIdx = currentStartIdx;
                    }
                    currentVal = arr[i];
                    currentLen = 1;
                    currentStartIdx = i;
                }
            }

            if (currentLen > maxLen)
            {
                maxLen = currentLen;
                maxVal = currentVal;
                maxStartIdx = currentStartIdx;
            }

            Console.WriteLine($"Значення: {maxVal}, Довжина: {maxLen}, Початковий індекс: {maxStartIdx}");
        }

        static void Task4_ShiftRight(int[] arr)
        {
            if (arr == null || arr.Length <= 1) return;

            int last = arr[arr.Length - 1];
            for (int i = arr.Length - 1; i > 0; i--)
            {
                arr[i] = arr[i - 1];
            }
            arr[0] = last;
        }

        static int[,] GenerateMatrix(int rows, int cols, int minVal, int maxVal, int seed)
        {
            Random rnd = new Random(seed);
            int[,] matrix = new int[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    matrix[i, j] = rnd.Next(minVal, maxVal + 1);
                }
            }
            return matrix;
        }

        static void Task5_ProcessMatrix(int[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            Console.WriteLine("Матриця:");
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Console.Write($"{matrix[i, j],4}");
                }
                Console.WriteLine();
            }

            int maxSum = int.MinValue;
            int maxSumRowIdx = 0;

            Console.WriteLine("\nСуми рядків:");
            for (int i = 0; i < rows; i++)
            {
                int rowSum = 0;
                for (int j = 0; j < cols; j++)
                {
                    rowSum += matrix[i, j];
                }
                Console.WriteLine($"Рядок {i}: сума = {rowSum}");

                if (rowSum > maxSum)
                {
                    maxSum = rowSum;
                    maxSumRowIdx = i;
                }
            }

            Console.WriteLine("\nМаксимуми стовпців:");
            for (int j = 0; j < cols; j++)
            {
                int colMax = matrix[0, j];
                for (int i = 1; i < rows; i++)
                {
                    if (matrix[i, j] > colMax)
                    {
                        colMax = matrix[i, j];
                    }
                }
                Console.WriteLine($"Стовпець {j}: макс = {colMax}");
            }

            Console.WriteLine($"\nРядок з найбільшою сумою: {maxSumRowIdx} (сума = {maxSum})");
        }

        static void RunEdgeCases()
        {
            int[] emptyArray = new int[0];
            int[] singleElementArray = new int[] { 5 };
            int[] sameElementsArray = new int[] { 3, 3, 3, 3 };

            Console.WriteLine("--- 1. Порожній масив ---");
            Task1_Aggregate(emptyArray);
            int[] fEmpty = Task2_FilterMultiplesOfThree(emptyArray);
            Console.Write("Фільтрація: "); PrintArray(fEmpty);
            Task3_LongestSeries(emptyArray);
            Task4_ShiftRight(emptyArray);

            Console.WriteLine("\n--- 2. Один елемент [5] ---");
            Task1_Aggregate(singleElementArray);
            int[] fSingle = Task2_FilterMultiplesOfThree(singleElementArray);
            Console.Write("Фільтрація: "); PrintArray(fSingle);
            Task3_LongestSeries(singleElementArray);
            Task4_ShiftRight(singleElementArray);
            Console.Write("Після зсуву: "); PrintArray(singleElementArray);

            Console.WriteLine("\n--- 3. Усі однакові [3, 3, 3, 3] ---");
            Task1_Aggregate(sameElementsArray);
            Task3_LongestSeries(sameElementsArray);
        }
    }
}