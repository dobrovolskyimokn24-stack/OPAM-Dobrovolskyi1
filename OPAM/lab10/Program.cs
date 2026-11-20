using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpamLab10
{
    internal class Program
    {
        private static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            string prosePath = "prose.txt";
            string prose = EnsureProseFile(prosePath, 55000);
            string repeats = new string('М', 10000);
            string patternTask3 = new string('М', 1069) + "Н";

            Console.WriteLine("=== ПАРАМЕТРИ ВАРІАНТА ===");
            Console.WriteLine("N = 7, K = 17, a = 1, b = 3, c = 2\n");

            RunTask1(prose);
            RunTask2(prose);
            RunTask3(prose, repeats, patternTask3);
            RunTask4();
            RunExtraC(prose);
            RunEdgeCases();
        }

        private static string EnsureProseFile(string path, int minLength)
        {
            if (File.Exists(path))
            {
                string existing = File.ReadAllText(path, Encoding.UTF8);
                if (existing.Length >= minLength)
                {
                    return existing;
                }
            }

            string[] paragraphs = new string[]
            {
                "Реве та стогне Дніпр широкий, сердитий вітер завива, додолу верби гне високі, горами хвилю підійма. І блідий місяць на ту пору із хмари де-де виглядав, неначе човен в синім морі, то виринав, то потопав.",
                "Садок вишневий коло хати, хрущі над вишнями гудуть, плугатарі з плугами йдуть, співають ідучи дівчата, а матері вечерять ждуть. Сем'я вечеря коло хати, вечірня зіронька встає.",
                "Заповіт. Як умру, то поховайте мене на могилі серед степу широкого на Вкраїні милій, щоб лани широкополі, і Дніпро, і кручі було видно, було чути, як реве ревучий.",
                "Ніч яка місячна, зоряна, ясная, видно, хоч голки збирай. Вийди, коханая, працею зморена, хоч на хвилиночку в гай. Сядемо вкупочці тут під калиною — і над панами я пан.",
                "Стоїть гора високая, попід горою гай, зелений гай, густесенький, неначе справді рай. Під гаєм в'ється річечка, як скло вода блищить, долиною широкою кудись вона біжить.",
                "Дивлюсь я на небо та й думку гадаю: чому я не сокіл, чому не літаю, чому мені, Боже, ти крилець не дав? Я б землю покинув і в небо злітав.",
                "Кохайтеся, чорнобриві, та не з москалями, бо москалі — чужі люде, роблять лихо з вами. Москаль любить жартуючи, жартуючи кине; піде в свою Московщину, а дівчина гине.",
                "На панщині пшеницю жала, втомилася; не спочивать пішла в снопи, пошкандибала Івана сина годувать. Воно сповитеє кричало у холодочку за снопом."
            };

            StringBuilder sb = new StringBuilder(minLength + 1000);
            int idx = 0;
            while (sb.Length < minLength)
            {
                sb.AppendLine(paragraphs[idx % paragraphs.Length]);
                idx++;
            }

            string result = sb.ToString();
            File.WriteAllText(path, result, Encoding.UTF8);
            return result;
        }

        private static void RunTask1(string prose)
        {
            Console.WriteLine("=== ЗАВДАННЯ 1: ТРИ ПАСТКИ ===");

            double t10kConcat = Benchmarks.MeasureMinMs(() => StringTasks.Trap1Concat(10000));
            double t10kBuilder = Benchmarks.MeasureMinMs(() => StringTasks.Trap1Builder(10000));
            double t100kConcat = Benchmarks.MeasureMinMs(() => StringTasks.Trap1Concat(100000));
            double t100kBuilder = Benchmarks.MeasureMinMs(() => StringTasks.Trap1Builder(100000));

            Console.WriteLine($"1. Конкатенація 10 000: += {t10kConcat:F4} мс | StringBuilder {t10kBuilder:F4} мс");
            Console.WriteLine($"   Конкатенація 100 000: += {t100kConcat:F4} мс | StringBuilder {t100kBuilder:F4} мс");

            string[] words = StringTasks.ExtractWords(prose);
            double tToLower = Benchmarks.MeasureMinMs(() => StringTasks.Trap2ToLower(words, 100000));
            double tOrdinal = Benchmarks.MeasureMinMs(() => StringTasks.Trap2OrdinalIgnoreCase(words, 100000));
            Console.WriteLine($"2. 100 000 порівнянь: ToLower {tToLower:F4} мс | OrdinalIgnoreCase {tOrdinal:F4} мс");

            string[] gheWords = StringTasks.GetWordsWithGhe();
            string[] ordSorted = StringTasks.SortOrdinal(gheWords);
            string[] culSorted = StringTasks.SortUkrainianCulture(gheWords);
            Console.WriteLine("3. Сортування слів із ґ:");
            Console.WriteLine("   Ordinal: " + string.Join(", ", ordSorted));
            Console.WriteLine("   uk-UA  : " + string.Join(", ", culSorted));
            Console.WriteLine();
        }

        private static void RunTask2(string prose)
        {
            Console.WriteLine("=== ЗАВДАННЯ 2: 8 НАЙЧАСТІШИХ СЛІВ ===");
            int[] sizes = new int[] { 1000, 10000, 50000 };

            Console.WriteLine($"{"Символів",10} | {"Наївно, мс",12} | {"Швидко, мс",12} | {"У скільки разів",16} | {"Збіг",6}");
            Console.WriteLine(new string('-', 66));

            for (int i = 0; i < sizes.Length; i++)
            {
                int sz = sizes[i];
                string slice = prose.Substring(0, sz);

                KeyValuePair<string, int>[] resNaive = null;
                KeyValuePair<string, int>[] resFast = null;

                double tNaive = Benchmarks.MeasureMinMs(() => { resNaive = StringTasks.TopKNaive(slice, 8); });
                double tFast = Benchmarks.MeasureMinMs(() => { resFast = StringTasks.TopKFast(slice, 8); });
                bool equal = StringTasks.AreTopKEqual(resNaive, resFast);
                double ratio = tFast > 0 ? tNaive / tFast : 0;

                Console.WriteLine($"{sz,10} | {tNaive,12:F4} | {tFast,12:F4} | {ratio,15:F1}x | {equal,6}");
            }
            Console.WriteLine();
        }

        private static void RunTask3(string prose, string repeats, string patternTask3)
        {
            Console.WriteLine("=== ЗАВДАННЯ 3: НАЇВНИЙ ПОШУК ПІДРЯДКА ===");
            string prose50k = prose.Substring(0, 50000);
            string existingLateWord = "пошкандибала";
            string missingWord = "синхрофазотрон";

            long cmp1 = 0, cmp2 = 0, cmp3 = 0;
            int idx1 = -1, idx2 = -1, idx3 = -1;

            double t1Naive = Benchmarks.MeasureMinMs(() => { idx1 = StringTasks.NaiveIndexOf(prose50k, existingLateWord, out cmp1); });
            double t1BuiltIn = Benchmarks.MeasureMinMs(() => { prose50k.IndexOf(existingLateWord, StringComparison.Ordinal); });

            double t2Naive = Benchmarks.MeasureMinMs(() => { idx2 = StringTasks.NaiveIndexOf(prose50k, missingWord, out cmp2); });
            double t2BuiltIn = Benchmarks.MeasureMinMs(() => { prose50k.IndexOf(missingWord, StringComparison.Ordinal); });

            double t3Naive = Benchmarks.MeasureMinMs(() => { idx3 = StringTasks.NaiveIndexOf(repeats, patternTask3, out cmp3); });
            double t3BuiltIn = Benchmarks.MeasureMinMs(() => { repeats.IndexOf(patternTask3, StringComparison.Ordinal); });

            Console.WriteLine($"Проза 50к (слово '{existingLateWord}', idx={idx1}): порівнянь = {cmp1}, наївний = {t1Naive:F4} мс, IndexOf = {t1BuiltIn:F4} мс");
            Console.WriteLine($"Проза 50к (немає '{missingWord}', idx={idx2}): порівнянь = {cmp2}, наївний = {t2Naive:F4} мс, IndexOf = {t2BuiltIn:F4} мс");
            Console.WriteLine($"Повтори 10к (шаблон 1069 М + Н, idx={idx3}): порівнянь = {cmp3}, наївний = {t3Naive:F4} мс, IndexOf = {t3BuiltIn:F4} мс");
            Console.WriteLine();
        }

        private static void RunTask4()
        {
            Console.WriteLine("=== ЗАВДАННЯ 4: РОЗРІЗАТИ СТРИЖЕНЬ 47 СМ ===");
            Random rng = new Random(7);
            int rodLength = 47;

            int optimalCount = 0;
            double worstLagPercent = 0.0;
            int[] smallestCounterexample = null;
            int ceGreedy = 0, ceBrute = 0;

            for (int t = 0; t < 100; t++)
            {
                int n = rng.Next(1, 16);
                int[] parts = new int[n];
                for (int i = 0; i < n; i++)
                {
                    parts[i] = rng.Next(3, 35);
                }

                int greedyWaste = StringTasksAndRodHelper(rodLength, parts, out int bruteWaste);
                if (greedyWaste == bruteWaste)
                {
                    optimalCount++;
                }
                else
                {
                    int usedGreedy = rodLength - greedyWaste;
                    int usedBrute = rodLength - bruteWaste;
                    double lag = usedBrute > 0 ? ((double)(usedBrute - usedGreedy) / usedBrute) * 100.0 : 0.0;
                    if (lag > worstLagPercent)
                    {
                        worstLagPercent = lag;
                    }

                    if (smallestCounterexample == null || parts.Length < smallestCounterexample.Length)
                    {
                        smallestCounterexample = parts;
                        ceGreedy = greedyWaste;
                        ceBrute = bruteWaste;
                    }
                }
            }

            Random rngBench = new Random(7);
            int[] parts15 = new int[15];
            for (int i = 0; i < 15; i++) parts15[i] = rngBench.Next(3, 35);

            int[] parts20 = new int[20];
            for (int i = 0; i < 20; i++) parts20[i] = rngBench.Next(3, 35);

            double time15 = Benchmarks.MeasureMinMs(() => RodCutting.BruteForceUnprunedMinWaste(rodLength, parts15));
            double time20 = Benchmarks.MeasureMinMs(() => RodCutting.BruteForceUnprunedMinWaste(rodLength, parts20));

            Console.WriteLine($"Тестів, де жадібне рішення дало оптимум: {optimalCount} / 100");
            Console.WriteLine($"Найгірше відставання: {worstLagPercent:F2}%");
            if (smallestCounterexample != null)
            {
                Console.WriteLine($"Найменший контрприклад ({smallestCounterexample.Length} ел.): [{string.Join(", ", smallestCounterexample)}] -> жадібно відхід {ceGreedy}, оптимум {ceBrute}");
            }
            Console.WriteLine($"Час перебору на 15 елементах: {time15:F4} мс | на 20 елементах: {time20:F4} мс");
            Console.WriteLine();
        }

        private static int StringTasksAndRodHelper(int rodLength, int[] parts, out int bruteWaste)
        {
            bruteWaste = RodCutting.BruteForceMinWaste(rodLength, parts);
            return RodCutting.GreedyMinWaste(rodLength, parts);
        }

        private static void RunExtraC(string prose)
        {
            Console.WriteLine("=== ДОДАТКОВО (ТАБЛИЦЯ C, c = 2): SUBSTRING ПРОТИ ІНДЕКСІВ ===");
            string text = prose.Substring(0, 50000);
            string pat = "пошкандибала";
            int allocCount = 0;

            double tSub = Benchmarks.MeasureMinMs(() => { StringTasks.ExtraCSubstringSearch(text, pat, out allocCount); });
            double tIdx = Benchmarks.MeasureMinMs(() => { StringTasks.ExtraCIndexSearch(text, pat); });

            Console.WriteLine($"Substring у циклі: {tSub:F4} мс (створено {allocCount} об'єктів string)");
            Console.WriteLine($"Порівняння по індексах: {tIdx:F4} мс (створено 0 об'єктів string)");
            Console.WriteLine();
        }

        private static void RunEdgeCases()
        {
            Console.WriteLine("=== ПЕРЕВІРКА КРАЙОВИХ ВИПАДКІВ ===");
            int c1;
            long cmp;
            Console.WriteLine($"1. Порожній рядок (зав. 2): {StringTasks.TopKFast("", 8).Length} слів");
            Console.WriteLine($"   Порожній рядок (зав. 3): індекс {StringTasks.NaiveIndexOf("", "а", out cmp)}");
            Console.WriteLine($"2. Один символ 'Я' (зав. 2): {StringTasks.TopKFast("Я", 8)[0].Key} -> {StringTasks.TopKFast("Я", 8)[0].Value}");
            Console.WriteLine($"3. Усі однакові 'ааааа' (зав. 2): {StringTasks.TopKFast("ааааа", 8)[0].Key} -> {StringTasks.TopKFast("ааааа", 8)[0].Value}");
            var trimmed = StringTasks.TopKFast("   слово    слово   ", 8);
            Console.WriteLine($"4. Пробіли по краях і подвійні (зав. 2): {trimmed[0].Key} -> {trimmed[0].Value}");
            Console.WriteLine($"5. Шаблон довший за текст (зав. 3): індекс {StringTasks.NaiveIndexOf("аб", "абвгд", out cmp)}");
            Console.WriteLine($"6. Порожній шаблон (зав. 3): індекс {StringTasks.NaiveIndexOf("текст", "", out cmp)}");
            c1 = RodCutting.BruteForceMinWaste(47, Array.Empty<int>());
            int c2 = RodCutting.BruteForceMinWaste(47, new int[] { 20 });
            Console.WriteLine($"7. 0 елементів (зав. 4): відхід {c1}; 1 елемент [20] (зав. 4): відхід {c2}");
        }
    }
}