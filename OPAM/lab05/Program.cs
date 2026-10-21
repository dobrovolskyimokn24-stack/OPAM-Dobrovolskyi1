Cusing System;
using System.Collections.Generic;

namespace Lab05
{
    public class CharStack
    {
        private char[] _items;
        private int _count;

        public CharStack(int initialCapacity)
        {
            _items = new char[initialCapacity];
            _count = 0;
        }

        public int Count => _count;
        public int Capacity => _items.Length;

        public void Push(char item)
        {
            if (_count == _items.Length)
            {
                char[] newItems = new char[_items.Length * 2];
                for (int i = 0; i < _items.Length; i++)
                {
                    newItems[i] = _items[i];
                }
                _items = newItems;
            }
            _items[_count++] = item;
        }

        public char Pop()
        {
            if (_count == 0)
                throw new InvalidOperationException("Стек порожній!");
            return _items[--_count];
        }

        public char Peek()
        {
            if (_count == 0)
                throw new InvalidOperationException("Стек порожній!");
            return _items[_count - 1];
        }
    }

    internal class Program
    {
        private const int N = 7;
        private const int K = 17;

        static void Main(string[] args)
        {
            Console.WriteLine($"N = {N}, K = {K}, a = {N % 3}, b = {N % 4}, c = {N % 5}\n");

            // Завдання 1
            Console.WriteLine("=== Завдання 1 · Перевірка дужок (Стандартний Stack) ===");
            string[] testStrings = new string[]
            {
                "(a[b]{c})",
                "([)]",
                ")(",
                "((",
                "",
                new string('(', 17) + new string(')', 16)
            };

            foreach (var str in testStrings)
            {
                var (isBal, errIdx) = CheckBracketsStandard(str);
                Console.WriteLine($"Рядок: \"{str}\" | Збалансований: {isBal} | Індекс помилки: {errIdx}");
            }
            Console.WriteLine();

            // Завдання 2
            Console.WriteLine("=== Завдання 2 · Зворотний польський запис ===");
            string[] rpnTests = new string[]
            {
                "3 4 + 2 *",
                "5 3 -",
                "8 2 /",
                "2 3 4 * +",
                "5 0 /",
                "10 7 + 5 2 max + 5 -"
            };

            foreach (var expr in rpnTests)
            {
                EvaluateRPN(expr);
            }
            Console.WriteLine();

            // Завдання 3
            Console.WriteLine("=== Завдання 3 · Черга обслуговування (Дві черги) ===");
            RunQueueSimulation();
            Console.WriteLine();

            // Завдання 4
            Console.WriteLine("=== Завдання 4 · Власний CharStack ===");
            int initialCap = K % 4 + 2;
            CharStack myStack = new CharStack(initialCap);
            Console.WriteLine($"Початкова місткість: {myStack.Capacity}");

            for (int i = 0; i < 5; i++)
            {
                myStack.Push((char)('A' + i));
                Console.WriteLine($"Push({(char)('A' + i)}) -> Count: {myStack.Count}, Capacity: {myStack.Capacity}");
            }

            Console.WriteLine("\nПеревірка Завдання 1 на власному CharStack:");
            foreach (var str in testStrings)
            {
                var (isBal, errIdx) = CheckBracketsCustom(str);
                Console.WriteLine($"Рядок: \"{str}\" | Збалансований: {isBal} | Індекс помилки: {errIdx}");
            }
            Console.WriteLine();

            // Крайові випадки
            Console.WriteLine("=== Крайові випадки ===");
            RunEdgeCases();
        }

        static (bool IsBalanced, int ErrorIndex) CheckBracketsStandard(string input)
        {
            Stack<(char Symbol, int Index)> stack = new Stack<(char, int)>();

            for (int i = 0; i < input.Length; i++)
            {
                char ch = input[i];

                if (ch == '(' || ch == '[' || ch == '{' || ch == '<')
                {
                    stack.Push((ch, i));
                }
                else if (ch == ')' || ch == ']' || ch == '}' || ch == '>')
                {
                    if (stack.Count == 0) return (false, i);

                    var top = stack.Pop();
                    if (!IsMatchingPair(top.Symbol, ch))
                    {
                        return (false, i);
                    }
                }
            }

            if (stack.Count > 0)
            {
                var firstUnclosed = stack.Pop();
                while (stack.Count > 0)
                {
                    firstUnclosed = stack.Pop();
                }
                return (false, firstUnclosed.Index);
            }

            return (true, -1);
        }

        static (bool IsBalanced, int ErrorIndex) CheckBracketsCustom(string input)
        {
            CharStack charStack = new CharStack(3);
            CharStack indexStack = new CharStack(3);

            for (int i = 0; i < input.Length; i++)
            {
                char ch = input[i];

                if (ch == '(' || ch == '[' || ch == '{' || ch == '<')
                {
                    charStack.Push(ch);
                    indexStack.Push((char)i);
                }
                else if (ch == ')' || ch == ']' || ch == '}' || ch == '>')
                {
                    if (charStack.Count == 0) return (false, i);

                    char topChar = charStack.Pop();
                    indexStack.Pop();

                    if (!IsMatchingPair(topChar, ch))
                    {
                        return (false, i);
                    }
                }
            }

            if (charStack.Count > 0)
            {
                int firstErrIdx = 0;
                while (indexStack.Count > 0)
                {
                    firstErrIdx = (int)indexStack.Pop();
                    charStack.Pop();
                }
                return (false, firstErrIdx);
            }

            return (true, -1);
        }

        static bool IsMatchingPair(char open, char close)
        {
            return (open == '(' && close == ')') ||
                   (open == '[' && close == ']') ||
                   (open == '{' && close == '}') ||
                   (open == '<' && close == '>');
        }

        static void EvaluateRPN(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                Console.WriteLine($"Вираз: \"{expression}\" -> Помилка: Порожній вираз.");
                return;
            }

            string[] tokens = expression.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            Stack<double> stack = new Stack<double>();

            foreach (string token in tokens)
            {
                if (double.TryParse(token, out double number))
                {
                    stack.Push(number);
                }
                else if (token == "+" || token == "-" || token == "*" || token == "/" || token == "min" || token == "max")
                {
                    if (stack.Count < 2)
                    {
                        Console.WriteLine($"Вираз: \"{expression}\" -> Помилка: Забракло операндів для операції '{token}'.");
                        return;
                    }

                    double b = stack.Pop();
                    double a = stack.Pop();

                    if (token == "+") stack.Push(a + b);
                    else if (token == "-") stack.Push(a - b);
                    else if (token == "*") stack.Push(a * b);
                    else if (token == "/")
                    {
                        if (b == 0)
                        {
                            Console.WriteLine($"Вираз: \"{expression}\" -> Помилка: Ділення на нуль.");
                            return;
                        }
                        stack.Push(a / b);
                    }
                    else if (token == "min") stack.Push(Math.Min(a, b));
                    else if (token == "max") stack.Push(Math.Max(a, b));
                }
                else
                {
                    Console.WriteLine($"Вираз: \"{expression}\" -> Помилка: Невідомий токен '{token}'.");
                    return;
                }
            }

            if (stack.Count == 1)
            {
                Console.WriteLine($"Вираз: \"{expression}\" -> Результат: {stack.Pop()}");
            }
            else
            {
                Console.WriteLine($"Вираз: \"{expression}\" -> Помилка: Забагато чисел у виразі.");
            }
        }

        struct Request
        {
            public int Id;
            public int Weight;
        }

        static void RunQueueSimulation()
        {
            int totalRequests = 8 + K % 5;
            Random rnd = new Random(N);

            Queue<Request> normalQueue = new Queue<Request>();
            Queue<Request> priorityQueue = new Queue<Request>();

            for (int i = 1; i <= totalRequests; i++)
            {
                Request req = new Request { Id = i, Weight = rnd.Next(1, 10) };
                if (i % 3 == 0)
                {
                    priorityQueue.Enqueue(req);
                }
                else
                {
                    normalQueue.Enqueue(req);
                }
            }

            int currentTime = 0;
            int maxQueueLength = Math.Max(normalQueue.Count, priorityQueue.Count);

            Console.WriteLine($"{"№ Заявки",-10} | {"Черга",-12} | {"Вага",-6} | {"Початок",-8} | {"Завершення",-10}");
            Console.WriteLine(new string('-', 55));

            while (priorityQueue.Count > 0)
            {
                Request req = priorityQueue.Dequeue();
                int startTime = currentTime;
                currentTime += req.Weight;
                Console.WriteLine($"{req.Id,-10} | {"Пріоритетна",-12} | {req.Weight,-6} | {startTime,-8} | {currentTime,-10}");
            }

            while (normalQueue.Count > 0)
            {
                Request req = normalQueue.Dequeue();
                int startTime = currentTime;
                currentTime += req.Weight;
                Console.WriteLine($"{req.Id,-10} | {"Звичайна",-12} | {req.Weight,-6} | {startTime,-8} | {currentTime,-10}");
            }

            Console.WriteLine($"\nЗагальний час обслуговування: {currentTime} хв");
            Console.WriteLine($"Максимальна довжина черги під час старту: {maxQueueLength}");
        }

        static void RunEdgeCases()
        {
            Console.WriteLine("1. Порожній рядок (дужки):");
            var (b1, i1) = CheckBracketsStandard("");
            Console.WriteLine($"   Збалансований: {b1}, Індекс: {i1}");

            Console.WriteLine("2. Рядок без дужок:");
            var (b2, i2) = CheckBracketsStandard("hello world");
            Console.WriteLine($"   Збалансований: {b2}, Індекс: {i2}");

            Console.WriteLine("3. Вираз з одного числа RPN:");
            EvaluateRPN("42");

            Console.WriteLine("4. Ділення на нуль:");
            EvaluateRPN("10 0 /");

            Console.WriteLine("5. Pop на порожньому CharStack:");
            try
            {
                CharStack emptyStack = new CharStack(3);
                emptyStack.Pop();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   Перехоплено виняток: {ex.Message}");
            }
        }
    }
}
