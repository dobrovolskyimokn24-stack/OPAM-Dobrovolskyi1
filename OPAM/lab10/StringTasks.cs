using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace OpamLab10
{
    public static class StringTasks
    {
        public static string Trap1Concat(int count)
        {
            string s = string.Empty;
            for (int i = 0; i < count; i++)
            {
                s += 'М';
            }
            return s;
        }

        public static string Trap1Builder(int count)
        {
            StringBuilder sb = new StringBuilder(count);
            for (int i = 0; i < count; i++)
            {
                sb.Append('М');
            }
            return sb.ToString();
        }

        public static int Trap2ToLower(string[] words, int iterations)
        {
            int matches = 0;
            int len = words.Length;
            for (int i = 0; i < iterations; i++)
            {
                string a = words[i % len];
                string b = words[(i + 1) % len];
                if (a.ToLower() == b.ToLower())
                {
                    matches++;
                }
            }
            return matches;
        }

        public static int Trap2OrdinalIgnoreCase(string[] words, int iterations)
        {
            int matches = 0;
            int len = words.Length;
            for (int i = 0; i < iterations; i++)
            {
                string a = words[i % len];
                string b = words[(i + 1) % len];
                if (string.Equals(a, b, StringComparison.OrdinalIgnoreCase))
                {
                    matches++;
                }
            }
            return matches;
        }

        public static string[] GetWordsWithGhe()
        {
            return new string[]
            {
                "ґанок", "ганок", "ґудзик", "гудзик", "ґрунт",
                "грунт", "ґава", "гава", "ґрати", "грати",
                "ґедзь", "гедзь", "ґлей", "глей", "ґніт",
                "гніт", "ґонт", "гонт", "ґуля", "гуля"
            };
        }

        public static string[] SortOrdinal(string[] input)
        {
            string[] copy = (string[])input.Clone();
            Array.Sort(copy, StringComparer.Ordinal);
            return copy;
        }

        public static string[] SortUkrainianCulture(string[] input)
        {
            string[] copy = (string[])input.Clone();
            Array.Sort(copy, StringComparer.Create(new CultureInfo("uk-UA"), false));
            return copy;
        }

        public static string[] ExtractWords(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return Array.Empty<string>();
            }

            List<string> words = new List<string>();
            StringBuilder current = new StringBuilder();

            for (int i = 0; i < text.Length; i++)
            {
                char ch = text[i];
                if (char.IsLetter(ch) || ch == '\'' || ch == '’')
                {
                    current.Append(char.ToUpperInvariant(ch));
                }
                else if (current.Length > 0)
                {
                    words.Add(current.ToString());
                    current.Clear();
                }
            }

            if (current.Length > 0)
            {
                words.Add(current.ToString());
            }

            return words.ToArray();
        }

        public static KeyValuePair<string, int>[] TopKNaive(string text, int k)
        {
            string[] words = ExtractWords(text);
            if (words.Length == 0 || k <= 0)
            {
                return Array.Empty<KeyValuePair<string, int>>();
            }

            List<string> uniqueWords = new List<string>();
            List<int> counts = new List<int>();

            for (int i = 0; i < words.Length; i++)
            {
                string w = words[i];
                bool seen = false;
                for (int u = 0; u < uniqueWords.Count; u++)
                {
                    if (string.Equals(uniqueWords[u], w, StringComparison.Ordinal))
                    {
                        seen = true;
                        break;
                    }
                }

                if (!seen)
                {
                    int freq = 0;
                    for (int j = 0; j < words.Length; j++)
                    {
                        if (string.Equals(words[j], w, StringComparison.Ordinal))
                        {
                            freq++;
                        }
                    }
                    uniqueWords.Add(w);
                    counts.Add(freq);
                }
            }

            KeyValuePair<string, int>[] result = new KeyValuePair<string, int>[uniqueWords.Count];
            for (int i = 0; i < uniqueWords.Count; i++)
            {
                result[i] = new KeyValuePair<string, int>(uniqueWords[i], counts[i]);
            }

            Array.Sort(result, CompareWordFreq);

            int take = Math.Min(k, result.Length);
            KeyValuePair<string, int>[] top = new KeyValuePair<string, int>[take];
            Array.Copy(result, top, take);
            return top;
        }

        public static KeyValuePair<string, int>[] TopKFast(string text, int k)
        {
            string[] words = ExtractWords(text);
            if (words.Length == 0 || k <= 0)
            {
                return Array.Empty<KeyValuePair<string, int>>();
            }

            Dictionary<string, int> dict = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int i = 0; i < words.Length; i++)
            {
                string w = words[i];
                if (dict.TryGetValue(w, out int count))
                {
                    dict[w] = count + 1;
                }
                else
                {
                    dict[w] = 1;
                }
            }

            KeyValuePair<string, int>[] result = new KeyValuePair<string, int>[dict.Count];
            int idx = 0;
            foreach (KeyValuePair<string, int> kv in dict)
            {
                result[idx++] = kv;
            }

            Array.Sort(result, CompareWordFreq);

            int take = Math.Min(k, result.Length);
            KeyValuePair<string, int>[] top = new KeyValuePair<string, int>[take];
            Array.Copy(result, top, take);
            return top;
        }

        private static int CompareWordFreq(KeyValuePair<string, int> a, KeyValuePair<string, int> b)
        {
            int cmp = b.Value.CompareTo(a.Value);
            if (cmp != 0)
            {
                return cmp;
            }
            return string.Compare(a.Key, b.Key, StringComparison.Ordinal);
        }

        public static bool AreTopKEqual(KeyValuePair<string, int>[] a, KeyValuePair<string, int>[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }
            for (int i = 0; i < a.Length; i++)
            {
                if (!string.Equals(a[i].Key, b[i].Key, StringComparison.Ordinal) || a[i].Value != b[i].Value)
                {
                    return false;
                }
            }
            return true;
        }

        public static int NaiveIndexOf(string text, string pat, out long comparisons)
        {
            comparisons = 0;
            if (text == null || pat == null)
            {
                return -1;
            }
            if (pat.Length == 0)
            {
                return 0;
            }
            if (pat.Length > text.Length)
            {
                return -1;
            }

            int limit = text.Length - pat.Length;
            for (int i = 0; i <= limit; i++)
            {
                int j = 0;
                while (j < pat.Length)
                {
                    comparisons++;
                    if (text[i + j] != pat[j])
                    {
                        break;
                    }
                    j++;
                }
                if (j == pat.Length)
                {
                    return i;
                }
            }
            return -1;
        }

        public static int ExtraCSubstringSearch(string text, string pat, out int allocatedStrings)
        {
            allocatedStrings = 0;
            if (pat.Length == 0)
            {
                return 0;
            }
            if (pat.Length > text.Length)
            {
                return -1;
            }

            int limit = text.Length - pat.Length;
            for (int i = 0; i <= limit; i++)
            {
                string sub = text.Substring(i, pat.Length);
                allocatedStrings++;
                if (string.Equals(sub, pat, StringComparison.Ordinal))
                {
                    return i;
                }
            }
            return -1;
        }

        public static int ExtraCIndexSearch(string text, string pat)
        {
            if (pat.Length == 0)
            {
                return 0;
            }
            if (pat.Length > text.Length)
            {
                return -1;
            }

            int limit = text.Length - pat.Length;
            for (int i = 0; i <= limit; i++)
            {
                int j = 0;
                while (j < pat.Length && text[i + j] == pat[j])
                {
                    j++;
                }
                if (j == pat.Length)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}