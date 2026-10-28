using System;
using System.Collections.Generic;

namespace lab07
{
    class Program
    {
        static void Main(string[] args)
        {
            int N = 7;
            int K = 19;
            int a = 1, b = 3, c = 2;

            Console.WriteLine($"=== ОПАМ ЛАБ 07 | N = {N}, K = {K}, a = {a}, b = {b}, c = {c} ===\n");

            var random = new Random(N);
            var numbers = new List<int>();
            for (int i = 0; i < 20; i++)
            {
                numbers.Add(random.Next(-K, K + 1));
            }

            var words = new List<string>
            {
                "яблуко", "автобус", "C-Sharp", "інтернет", "апельсин", "дефіс-слово",
                "код", "делегати", "ОПАМ", "огірок", "елемент", "ідея"
            };

            var students = new List<Student>
            {
                new Student { Name = "Шевченко", Group = "КН-3/1", Score = 85 },
                new Student { Name = "Коваленко", Group = "КН-3/1", Score = 55 },
                new Student { Name = "Бондаренко", Group = "КН-3/2", Score = 92 },
                new Student { Name = "Бойко", Group = "КН-3/1", Score = 70 },
                new Student { Name = "Ткаченко", Group = "КН-3/2", Score = 48 },
                new Student { Name = "Петренко", Group = "КН-3/1", Score = 60 }
            };

            // ЗАВДАННЯ 1: FILTER
            Console.WriteLine("--- ЗАВДАННЯ 1: FILTER ---");
            var filteredNums = Algorithms.Filter(numbers, x => x % 3 == 0);
            Console.WriteLine($"Числа кратні 3 ({filteredNums.Count}): {string.Join(", ", filteredNums)}");

            var vowels = "аеєиіїоуюяАЕЄИІЇОУЮЯ";
            var filteredWords = Algorithms.Filter(words, w => w.Length > 0 && vowels.Contains(w[0]));
            Console.WriteLine($"Рядки на голосну ({filteredWords.Count}): {string.Join(", ", filteredWords)}");

            var filteredStudents = Algorithms.Filter(students, s => s.Group == "КН-3/1");
            Console.WriteLine($"Студенти з КН-3/1 ({filteredStudents.Count}):");
            foreach (var s in filteredStudents) Console.WriteLine($"  {s}");

            // ЗАВДАННЯ 2: MAP
            Console.WriteLine("\n--- ЗАВДАННЯ 2: MAP ---");
            var mappedNums = Algorithms.Map(numbers, x => $"({x})");
            Console.WriteLine($"Числа у дужках: {string.Join(", ", mappedNums)}");

            var mappedWords = Algorithms.Map(words, w => {
                int count = 0;
                foreach (char ch in w) if (vowels.Contains(ch)) count++;
                return count;
            });
            Console.WriteLine($"Кількість голосних у словах: {string.Join(", ", mappedWords)}");

            var mappedStudents = Algorithms.Map(students, s => s.Score >= 60);
            Console.WriteLine($"Студенти склали (>= 60): {string.Join(", ", mappedStudents)}");

            // ЗАВДАННЯ 3: AGGREGATE
            Console.WriteLine("\n--- ЗАВДАННЯ 3: AGGREGATE ---");
            int sum = Algorithms.Aggregate(numbers, 0, (acc, x) => acc + x);
            Console.WriteLine($"1. Сума чисел: {sum}");

            long product = Algorithms.Aggregate(numbers, 1L, (acc, x) => acc * x);
            Console.WriteLine($"2. Добуток чисел: {product}");

            int max = numbers.Count > 0 
                ? Algorithms.Aggregate(numbers, numbers[0], (acc, x) => x > acc ? x : acc) 
                : 0;
            Console.WriteLine($"3. Максимум чисел: {max}");

            string concatenatedWords = Algorithms.Aggregate(words, "", (acc, w) => acc == "" ? w : acc + ", " + w);
            Console.WriteLine($"4. Конкатенація рядків (c = 2): {concatenatedWords}");

            // ЗАВДАННЯ 4: GROUPBY
            Console.WriteLine("\n--- ЗАВДАННЯ 4: GROUP BY ---");
            var byGroup = Algorithms.GroupBy(students, s => s.Group);
            Console.WriteLine("Студенти за групою:");
            foreach (var pair in byGroup) Console.WriteLine($"  Група '{pair.Key}': {pair.Value.Count} осіб");

            var byFirstLetter = Algorithms.GroupBy(students, s => s.Name[0]);
            Console.WriteLine("Студенти за першою літерою прізвища:");
            foreach (var pair in byFirstLetter) Console.WriteLine($"  Літера '{pair.Key}': {pair.Value.Count} осіб");

            var byScoreRange = Algorithms.GroupBy(students, s => s.Score switch {
                >= 90 => "90-100",
                >= 75 => "75-89",
                >= 60 => "60-74",
                _ => "0-59"
            });
            Console.WriteLine("Студенти за діапазоном балу:");
            foreach (var pair in byScoreRange) Console.WriteLine($"  Діапазон '{pair.Key}': {pair.Value.Count} осіб");

            var wordsByLength = Algorithms.GroupBy(words, w => w.Length);
            Console.WriteLine("Рядки за довжиною:");
            foreach (var pair in wordsByLength) Console.WriteLine($"  Довжина {pair.Key}: {pair.Value.Count} слів");

            // ЗАВДАННЯ 5: CAPTURING
            Console.WriteLine("\n--- ЗАВДАННЯ 5: CAPTURING ---");
            int threshold = K;
            Func<int, bool> checkThreshold = x => x > threshold;
            threshold = 1000;
            Console.WriteLine($"1. Змінна після лямбди: {checkThreshold(5)}");

            var funcsFor = new List<Func<int>>();
            for (int i = 0; i < 3; i++) funcsFor.Add(() => i);
            Console.Write("2. Цикл for: ");
            foreach (var f in funcsFor) Console.Write($"{f()} ");
            Console.WriteLine();

            var funcsForeach = new List<Func<int>>();
            foreach (var item in new int[] { 0, 1, 2 }) funcsForeach.Add(() => item);
            Console.Write("3. Цикл foreach: ");
            foreach (var f in funcsForeach) Console.Write($"{f()} ");
            Console.WriteLine();
        }
    }
}
