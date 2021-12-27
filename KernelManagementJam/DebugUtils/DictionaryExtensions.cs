using System;
using System.Collections.Generic;

namespace KernelManagementJam
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
        
        public static Dictionary<TKey, TSource> SafeToDictionary<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof (source));
            if (keySelector == null)
                throw new ArgumentNullException(nameof (keySelector));
            
            Dictionary<TKey, TSource> dictionary = new Dictionary<TKey, TSource>();

            foreach (TSource source1 in source)
                dictionary[keySelector(source1)] = source1;
            
            return dictionary;

        }

    }
}