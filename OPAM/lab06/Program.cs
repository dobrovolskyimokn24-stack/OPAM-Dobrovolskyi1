using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lab06
{
    class Student
    {
        public string Name;
        public string Group;
        public int Score;

        public Student(string name, string group, int score)
        {
            Name = name;
            Group = group;
            Score = score;
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
            Console.WriteLine("=== Завдання 1 · Частоти літер ===");
            string text = "Алгоритм описує послідовність дій. Алгоритм можна записати словами, " +
                         "а можна записати кодом. Кодом алгоритм стає тоді, коли його " +
                         "виконує машина, а не людина. Одна задача — багато алгоритмів.";
            
            int threshold = K % 3 + 2;
            var (charCounts, totalUnique) = CountCharFrequencies(text);
            Console.WriteLine($"Загальна кількість різних літер: {totalUnique}");
            Console.WriteLine($"Літери з частотою >= {threshold}:");
            foreach (var kvp in charCounts)
            {
                if (kvp.Value >= threshold)
                {
                    Console.WriteLine($"  '{kvp.Key}': {kvp.Value}");
                }
            }
            Console.WriteLine();

            // Завдання 2
            Console.WriteLine("=== Завдання 2 · Групування за кількістю літер у прізвищі ===");
            List<Student> students = GetSampleStudents();
            var groupedStudents = GroupStudentsByNameLength(students);

            foreach (var kvp in groupedStudents)
            {
                double avgScore = CalculateAverageScore(kvp.Value);
                Console.WriteLine($"Ключ (довжина): {kvp.Key} | Кількість: {kvp.Value.Count} | Середній бал: {avgScore:F2}");
                foreach (var st in kvp.Value)
                {
                    Console.WriteLine($"   - {st.Name} ({st.Group}): {st.Score}");
                }
            }
            Console.WriteLine();

            // Завдання 3
            Console.WriteLine("=== Завдання 3 · Вилучення дублікатів ===");
            List<int> origList = GenerateRandomList(25, 1, 13, N);
            Console.Write("Початковий список: ");
            PrintList(origList);

            HashSet<int> unorderedSet = RemoveDuplicatesUnordered(origList);
            Console.Write("Без збереження порядку (HashSet): ");
            PrintCollection(unorderedSet);

            List<int> orderedList = RemoveDuplicatesOrdered(origList);
            Console.Write("Зі збереженням порядку:           ");
            PrintList(orderedList);
            Console.WriteLine();

            // Завдання 4
            Console.WriteLine("=== Завдання 4 · Елементи лише в одному з двох наборів ===");
            List<int> setA_list = GenerateRandomList(30, 1, 37, N);
            List<int> setB_list = GenerateRandomList(30, 1, 37, N + 100);

            List<int> resultHashSet = SymmetricDifferenceHashSet(setA_list, setB_list);
            List<int> resultLists = SymmetricDifferenceLists(setA_list, setB_list);

            Console.Write("Результат через HashSet: ");
            PrintList(resultHashSet);
            Console.Write("Результат через списки: ");
            PrintList(resultLists);
            Console.WriteLine($"Результати збігаються: {AreListsEqualUnordered(resultHashSet, resultLists)}\n");

            // Завдання 5
            Console.WriteLine("=== Завдання 5 · Заміри швидкодії ===");
            RunBenchmarks();
            Console.WriteLine();

            // Крайові випадки
            Console.WriteLine("=== Крайові випадки ===");
            RunEdgeCases();
        }

        static (Dictionary<char, int>, int) CountCharFrequencies(string input)
        {
            Dictionary<char, int> dict = new Dictionary<char, int>();
            for (int i = 0; i < input.Length; i++)
            {
                char ch = input[i];
                if (char.IsLetter(ch))
                {
                    ch = char.ToLower(ch);
                    if (dict.ContainsKey(ch))
                    {
                        dict[ch] = dict[ch] + 1;
                    }
                    else
                    {
                        dict.Add(ch, 1);
                    }
                }
            }
            return (dict, dict.Count);
        }

        static List<Student> GetSampleStudents()
        {
            return new List<Student>
            {
                new Student("Коваленко", "КН-3/1", 88),
                new Student("Мельник",   "КН-3/2", 64),
                new Student("Іщенко",    "КН-3/1", 93),
                new Student("Бондар",    "КН-3/1", 71),
                new Student("Савченко",  "КН-3/2", 55),
                new Student("Ткачук",    "КН-3/2", 90),
                new Student("Кравець",   "КН-3/1", 76),
                new Student("Іванів",    "КН-3/1", 48),
                new Student("Мороз",     "КН-3/2", 82),
                new Student("Шевчук",    "КН-3/1", 67),
                new Student("Бойко",     "КН-3/2", 95),
                new Student("Кушнір",    "КН-3/2", 73)
            };
        }

        static Dictionary<int, List<Student>> GroupStudentsByNameLength(List<Student> students)
        {
            Dictionary<int, List<Student>> groups = new Dictionary<int, List<Student>>();
            for (int i = 0; i < students.Count; i++)
            {
                Student s = students[i];
                int key = s.Name.Length;
                if (!groups.ContainsKey(key))
                {
                    groups.Add(key, new List<Student>());
                }
                groups[key].Add(s);
            }
            return groups;
        }

        static double CalculateAverageScore(List<Student> list)
        {
            if (list == null || list.Count == 0) return 0;
            int sum = 0;
            for (int i = 0; i < list.Count; i++)
            {
                sum += list[i].Score;
            }
            return (double)sum / list.Count;
        }

        static HashSet<int> RemoveDuplicatesUnordered(List<int> list)
        {
            return new HashSet<int>(list);
        }

        static List<int> RemoveDuplicatesOrdered(List<int> list)
        {
            HashSet<int> seen = new HashSet<int>();
            List<int> result = new List<int>();
            for (int i = 0; i < list.Count; i++)
            {
                int item = list[i];
                if (seen.Add(item))
                {
                    result.Add(item);
                }
            }
            return result;
        }

        static List<int> SymmetricDifferenceHashSet(List<int> listA, List<int> listB)
        {
            HashSet<int> setA = new HashSet<int>(listA);
            HashSet<int> setB = new HashSet<int>(listB);

            HashSet<int> diffA = new HashSet<int>(setA);
            diffA.ExceptWith(setB
