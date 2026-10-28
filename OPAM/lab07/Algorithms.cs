using System;
using System.Collections.Generic;

namespace lab07
{
    public static class Algorithms
    {
        public static List<T> Filter<T>(List<T> src, Func<T, bool> keep)
        {
            var result = new List<T>();
            foreach (var item in src)
            {
                if (keep(item))
                {
                    result.Add(item);
                }
            }
            return result;
        }

        public static List<TOut> Map<TIn, TOut>(List<TIn> src, Func<TIn, TOut> convert)
        {
            var result = new List<TOut>();
            foreach (var item in src)
            {
                result.Add(convert(item));
            }
            return result;
        }

        public static TAcc Aggregate<T, TAcc>(List<T> src, TAcc start, Func<TAcc, T, TAcc> step)
        {
            TAcc result = start;
            foreach (var item in src)
            {
                result = step(result, item);
            }
            return result;
        }

        public static Dictionary<TKey, List<T>> GroupBy<T, TKey>(List<T> src, Func<T, TKey> keyOf) where TKey : notnull
        {
            var dict = new Dictionary<TKey, List<T>>();
            foreach (var item in src)
            {
                TKey key = keyOf(item);
                if (!dict.ContainsKey(key))
                {
                    dict[key] = new List<T>();
                }
                dict[key].Add(item);
            }
            return dict;
        }
    }
}