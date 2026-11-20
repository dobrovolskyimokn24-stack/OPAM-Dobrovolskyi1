using System;

namespace OpamLab10
{
    public static class RodCutting
    {
        public static int GreedyMinWaste(int rodLength, int[] parts)
        {
            if (parts == null || parts.Length == 0 || rodLength <= 0)
            {
                return rodLength;
            }

            int[] sorted = (int[])parts.Clone();
            Array.Sort(sorted);
            Array.Reverse(sorted);

            int remaining = rodLength;
            for (int i = 0; i < sorted.Length; i++)
            {
                if (sorted[i] > 0 && sorted[i] <= remaining)
                {
                    remaining -= sorted[i];
                }
            }
            return remaining;
        }

        public static int BruteForceMinWaste(int rodLength, int[] parts)
        {
            if (parts == null || parts.Length == 0 || rodLength <= 0)
            {
                return rodLength;
            }

            int bestWaste = rodLength;
            SearchRecursive(0, 0, rodLength, parts, ref bestWaste);
            return bestWaste;
        }

        private static void SearchRecursive(int index, int currentSum, int rodLength, int[] parts, ref int bestWaste)
        {
            int waste = rodLength - currentSum;
            if (waste < bestWaste)
            {
                bestWaste = waste;
            }
            if (bestWaste == 0 || index == parts.Length)
            {
                return;
            }

            if (parts[index] > 0 && currentSum + parts[index] <= rodLength)
            {
                SearchRecursive(index + 1, currentSum + parts[index], rodLength, parts, ref bestWaste);
            }

            SearchRecursive(index + 1, currentSum, rodLength, parts, ref bestWaste);
        }

        public static int BruteForceUnprunedMinWaste(int rodLength, int[] parts)
        {
            if (parts == null || parts.Length == 0 || rodLength <= 0)
            {
                return rodLength;
            }

            int bestWaste = rodLength;
            SearchUnpruned(0, 0, rodLength, parts, ref bestWaste);
            return bestWaste;
        }

        private static void SearchUnpruned(int index, int currentSum, int rodLength, int[] parts, ref int bestWaste)
        {
            if (index == parts.Length)
            {
                if (currentSum <= rodLength)
                {
                    int waste = rodLength - currentSum;
                    if (waste < bestWaste)
                    {
                        bestWaste = waste;
                    }
                }
                return;
            }

            SearchUnpruned(index + 1, currentSum + parts[index], rodLength, parts, ref bestWaste);
            SearchUnpruned(index + 1, currentSum, rodLength, parts, ref bestWaste);
        }
    }
}