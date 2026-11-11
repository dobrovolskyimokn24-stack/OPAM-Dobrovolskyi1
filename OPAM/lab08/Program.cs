using System;

class Program
{
    static string[] alphabet = { "А", "Б", "В", "Г", "Ґ", "Д", "Е", "Є", "Ж", "З", "И", "І", "Ї", "Й", "К", "Л", "М", "Н", "О", "П", "Р", "С", "Т", "У", "Ф", "Х", "Ц", "Ч", "Ш", "Щ", "Ь", "Ю", "Я" };

    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        int N = 7;
        string X = "М-00";
        string Y = "М-99";
        string missing = "Я-99";

      Func<string, string, int> cmp = (s1, s2) => s1.CompareTo(s2);

        string[] rawData = GenerateData(100000, N);
        string[] sortedData = (string[])rawData.Clone();
        Array.Sort(sortedData, (s1, s2) => cmp(s1, s2));

        string firstElem = sortedData[0];

        Console.WriteLine($"=== ЛАБОРАТОРНА РОБОТА 8 | N={N} ===\n");

        // 1. Лінійний пошук
        Console.WriteLine("--- Завдання 1: Лінійний пошук ---");
        Search.LinearSearch(sortedData, X, cmp, out int c1_1);
        Console.WriteLine($"1. X ('{X}'): {c1_1} порівнянь");
        Search.LinearSearch(sortedData, firstElem, cmp, out int c1_2);
        Console.WriteLine($"2. Перший ('{firstElem}'): {c1_2} порівнянь");
        Search.LinearSearch(sortedData, missing, cmp, out int c1_3);
        Console.WriteLine($"3. Відсутній ('{missing}'): {c1_3} порівнянь\n");

        // 2. Бінарний пошук та поломки
        Console.WriteLine("--- Завдання 2: Бінарний пошук і поломки ---");
        Search.BinarySearch(sortedData, X, cmp, out int c2_1);
        Search.BinarySearch(sortedData, firstElem, cmp, out int c2_2);
        Search.BinarySearch(sortedData, missing, cmp, out int c2_3);

        Console.WriteLine($"Бінарний пошук: X={c2_1}, First={c2_2}, Missing={c2_3} порівнянь.");

        int b1_idx = Broken.Broken1_Condition(sortedData, sortedData[sortedData.Length - 1], cmp, out _);
        Console.WriteLine($"Поломка 1 (lo < hi): Останній елемент повертає індекс {b1_idx}");

        Broken.Broken2_NoIncrement(sortedData, "М-50", cmp, out int b2_c);
        Console.WriteLine($"Поломка 2 (lo = mid): Цикл не закінчується (зупинено лічильником на {b2_c} кроці)");

        int b3_idx = Broken.Broken3_Overflow(sortedData, X, cmp, out int b3_c);
        Console.WriteLine($"Поломка 3 (mid=(lo+hi)/2): На 100 000 елементах працює без переповнення, індекс {b3_idx}");

        int missingCount = 0;
        Random rnd = new Random(N);
        for (int i = 0; i < 100; i++)
        {
            string t = rawData[rnd.Next(rawData.Length)];
            if (Broken.Broken4_UnsortedSearch(rawData, t, cmp, out _) == -1) missingCount++;
        }
        Console.WriteLine($"Поломка 4 (Несорт. масив): З 100 наявних не знайдено {missingCount}\n");

        // 3. Пошук межі
        Console.WriteLine("--- Завдання 3: Пошук межі ---");
        int lb = Search.BoundSearch(sortedData, elem => cmp(elem, X) >= 0, out int c_lb);
        int ub = Search.BoundSearch(sortedData, elem => cmp(elem, Y) > 0, out int c_ub);
        Console.WriteLine($"LowerBound('{X}'): індекс {lb} ({c_lb} порівнянь)");
        Console.WriteLine($"UpperBound('{Y}'): індекс {ub} ({c_ub} порівнянь)");
        Console.WriteLine($"Кількість елементів у [{X}; {Y}]: {ub - lb}\n");

        // 4. Потрійний пошук
        Console.WriteLine("--- Завдання 4: Потрійний пошук ---");
        Search.TernarySearch(sortedData, X, cmp, out int c4_1);
        Search.TernarySearch(sortedData, missing, cmp, out int c4_3);
        Console.WriteLine($"TernarySearch: X={c4_1} ({c4_1} порівнянь), Missing={c4_3} порівнянь.\n");

        // Крайові випадки
        Console.WriteLine("--- Крайові випадки ---");
        string[] emptyArr = new string[0];
        Console.WriteLine($"Порожній масив: {Search.BinarySearch(emptyArr, "А-00", cmp, out _)}");
        string[] singleArr = { "М-00" };
        Console.WriteLine($"1 елемент: {Search.BinarySearch(singleArr, "М-00", cmp, out _)}");
    }

    static string[] GenerateData(int size, int seed)
    {
        Random rnd = new Random(seed);
        string[] data = new string[size];
        for (int i = 0; i < size; i++)
        {
            string letter = alphabet[rnd.Next(alphabet.Length)];
            int num = rnd.Next(0, 100);
            data[i] = $"{letter}-{num:D2}";
        }
        return data;
    }
}