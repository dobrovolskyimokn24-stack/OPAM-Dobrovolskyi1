using System;
using System.Collections.Generic;

public static class Verification
{
    public static bool IsSorted<T>(T[] a, Func<T, T, int> cmp)
    {
        for (int i = 0; i < a.Length - 1; i++)
        {
            if (cmp(a[i], a[i + 1]) > 0) return false;
        }
        return true;
    }

    public static bool IsPermutationOf<T>(T[] original, T[] sorted) where T : notnull
    {
        if (original.Length != sorted.Length) return false;

        var dict = new Dictionary<T, int>();
        foreach (var item in original)
        {
            if (dict.ContainsKey(item)) dict[item]++;
            else dict[item] = 1;
        }

        foreach (var item in sorted)
        {
            if (!dict.ContainsKey(item) || dict[item] == 0) return false;
            dict[item]--;
        }

        return true;
    }
}

public class Student
{
    public string Name { get; set; }
    public string Group { get; set; }
    public int Score { get; set; }

    public Student(string name, string group, int score)
    {
        Name = name;
        Group = group;
        Score = score;
    }
}