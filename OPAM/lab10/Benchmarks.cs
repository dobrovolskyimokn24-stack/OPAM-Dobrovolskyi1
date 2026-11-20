using System;
using System.Diagnostics;

namespace OpamLab10
{
    public static class Benchmarks
    {
        public static double MeasureMinMs(Action action)
        {
            action();

            double bestMs = double.MaxValue;
            Stopwatch sw = new Stopwatch();
            for (int i = 0; i < 3; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                sw.Restart();
                action();
                sw.Stop();
                double elapsed = sw.Elapsed.TotalMilliseconds;
                if (elapsed < bestMs)
                {
                    bestMs = elapsed;
                }
            }
            return bestMs;
        }
    }
}