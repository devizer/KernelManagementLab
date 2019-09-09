using System;
using System.Collections.Generic;

namespace Universe.Dashboard.Agent
{
    public static class DictionaryExtensions
    {
        public static V GetOrAdd<K, V>(this IDictionary<K, V> dictionary, K key, Func<K,V> getNewValue)
        {
            if (dictionary.TryGetValue(key, out var ret))
                return ret;

            ret = getNewValue(key);
            dictionary.Add(key, ret);
            return ret;
        }
    }
}