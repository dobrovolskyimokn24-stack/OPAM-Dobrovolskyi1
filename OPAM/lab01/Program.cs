using System;

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding = System.Text.Encoding.UTF8;

        int N = 7;
        int K = 17;

        int a = N % 3; // 1
        int b = N % 4; // 3
        int c = N % 5; // 2

        while (true)
        {
            Console.WriteLine($"\n--- OPAM Лабораторна №1 ---");
            Console.WriteLine($"Варіант: N = {N}, K = {K}, a = {a}, b = {b}, c = {c}");
            Console.WriteLine("1. Завдання 1 (Перевірка дати)");
            Console.WriteLine("2. Завдання 2 (Обробка послідовності)");
            Console.WriteLine("3. Завдання 4 (НСД та порівняння ітерацій)");
            Console.WriteLine("0. Вихід");
            Console.Write("Виберіть завдання: ");

            string choice = Console.ReadLine();
            Console.WriteLine();

            if (choice == "1") Task1();
            else if (choice == "2") Task2(K);
            else if (choice == "4") Task4(N);
            else if (choice == "0") break;
            else Console.WriteLine("Некоректний вибір!");
        }
    }

    static void Task1()
    {
        Console.WriteLine("--- Завдання 1: Перевірка дати ---");
        int day = ReadInt("Введіть день: ");
        int month = ReadInt("Введіть місяць: ");
        int year = ReadInt("Введіть рік: ");

        if (year < 1 || month < 1 || month > 12 || day < 1)
        {
            Console.WriteLine("Помилка: Некоректна дата!");
            return;
        }

        bool isLeap = (year % 4 == 0 && year % 100 != 0) || (year % 400 == 0);
        int daysInMonth = 31;

        if (month == 4 || month == 6 || month == 9 || month == 11)
        {
            daysInMonth = 30;
        }
        else if (month == 2)
        {
            daysInMonth = isLeap ? 29 : 28;
        }

        if (day > daysInMonth)
        {
            Console.WriteLine("Помилка: Некоректна дата!");
            return;
        }

        Console.WriteLine("Дата коректна!");

        string season = "";
        if (month == 12 || month == 1 || month == 2) season = "Зима";
        else if (month >= 3 && month <= 5) season = "Весна";
        else if (month >= 6 && month <= 8) season = "Літо";
        else season = "Осінь";

        int dayOfYear = day;
        for (int m = 1; m < month; m++)
        {
            if (m == 4 || m == 6 || m == 9 || m == 11) dayOfYear += 30;
            else if (m == 2) dayOfYear += isLeap ? 29 : 28;
            else dayOfYear += 31;
        }

        Console.WriteLine($"Пора року: {season}");
        Console.WriteLine($"Номер дня від початку року: {dayOfYear}");
    }

    static void Task2(int K)
    {
        Console.WriteLine($"--- Завдання 2: Обробка послідовності (кратні {K}, добуток) ---");
        Console.WriteLine("Вводьте цілі числа (0 для завершення):");

        long sumAll = 0;
        int zeroCount = 0;
        long product = 1;
        bool foundAny = false;

        while (true)
        {
            int num = ReadInt("> ");
            if (num == 0)
            {
                zeroCount++;
                sumAll += num;
                break;
            }

            sumAll += num;

            if (num % K == 0)
            {
                product *= num;
                foundAny = true;
            }
        }

        Console.WriteLine($"Сума всіх елементів: {sumAll}");
        Console.WriteLine($"Кількість нулів: {zeroCount}");

        if (foundAny)
        {
            Console.WriteLine($"Добуток елементів, кратних {K}: {product}");
        }
        else
        {
            Console.WriteLine($"Добуток елементів, кратних {K}: Немає даних (числа не вводилися)");
        }
    }

    static void Task4(int N)
    {
        Console.WriteLine("--- Завдання 4: НСД чисел ---");
        int A = 1000000 + N; // 1000007
        int B = 999999 - N;  // 999992

        Console.WriteLine($"A = {A}, B = {B}");

        long iterBrute = 0;
        int gcdBrute = 1;
        int min = A < B ? A : B;
        for (int i = min; i >= 1; i--)
        {
            iterBrute++;
            if (A % i == 0 && B % i == 0)
            {
                gcdBrute = i;
                break;
            }
        }

        long iterEuclid = 0;
        int tempA = A;
        int tempB = B;
        while (tempB != 0)
        {
            iterEuclid++;
            int rem = tempA % tempB;
            tempA = tempB;
            tempB = rem;
        }
        int gcdEuclid = tempA;

        Console.WriteLine($"1. Перебір дільників: НСД = {gcdBrute}, Ітерацій = {iterBrute}");
        Console.WriteLine($"2. Алгоритм Евкліда:   НСД = {gcdEuclid}, Ітерацій = {iterEuclid}");
    }

    static int ReadInt(string prompt)
    {
        int result;
        while (true)
        {
            Console.Write(prompt);
            string input = Console.ReadLine();
            if (int.TryParse(input, out result))
            {
                return result;
            }
            Console.WriteLine("Помилка: Введіть коректне ціле число!");
        }
    }
}