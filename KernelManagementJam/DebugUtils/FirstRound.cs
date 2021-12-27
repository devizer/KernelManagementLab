using System;
using System.Collections.Generic;

namespace KernelManagementJam
{
    public static class FirstRound
    {
        static Dictionary<string,int> Counters = new Dictionary<string, int>(StringComparer.Ordinal);
        static readonly object Sync = new object();
        public static void RunOnly(this Action action, int count = 1, string pathKey = "undefined")
        {
            int num;
            lock (Sync)
            {
                num = Counters.GetOrAdd(pathKey, x => 0);
                num++;
                if (num <= count)
                    Counters[pathKey] = num;
            }

            if (num <= count) action();
        }

        public static void RunOnce(this Action action, string key)
        {
            RunOnly(action, 1, key);
        }

        public static void RunTwice(this Action action, string key)
        {
            RunOnly(action, 2, key);
        }
    }
}