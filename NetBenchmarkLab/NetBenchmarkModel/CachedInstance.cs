using System;
using System.Diagnostics;

namespace NetBenchmarkLab.NetBenchmarkModel
{
    class CachedInstance<T>  
    {
        private readonly int TTL;
        private readonly Func<T> Implementation;
        private T _Value;
        private bool _HasValue;
        static readonly object Sync = new object(); 
        private Stopwatch Stopwatch = null;

        public CachedInstance(int ttl, Func<T> implementation)
        {
            TTL = ttl;
            Implementation = implementation;
        }

        public T Value
        {
            get
            {
                if (_HasValue && Stopwatch != null && Stopwatch.ElapsedMilliseconds <= TTL)
                    return _Value;
                
                lock (Sync)
                {
                    if (_HasValue) return _Value;
                    var settings = Implementation();
                    Stopwatch = Stopwatch ?? Stopwatch.StartNew();
                    Stopwatch.Restart();
                    _Value = settings;
                    _HasValue = true;
                    return _Value;
                }
            }
        }

    }
}