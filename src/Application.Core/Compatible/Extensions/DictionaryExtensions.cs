using System.Collections.Concurrent;

namespace Application.Core.Compatible.Extensions
{
    public static class DictionaryExtensions
    {
        public static TValue? get<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue? defaultValue = default) where TValue : struct where TKey : notnull
        {
            return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
        }

        public static TValue? remove<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TKey : notnull where TValue : struct
        {
            if (dictionary.Remove(key, out var v))
                return v;

            return null;
        }

        public static void putAll<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Dictionary<TKey, TValue> otherDic) where TKey : notnull
        {
            foreach (var item in otherDic)
            {
                dictionary.AddOrUpdate(item.Key, item.Value);
            }
        }

        /// <summary>
        /// 添加或更新，返回旧值
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TValue? AddOrUpdateReturnOldValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value) where TKey : notnull
        {
            var oldValue = dictionary.ContainsKey(key) ? dictionary[key] : default(TValue);
            dictionary[key] = value;
            return oldValue;
        }

        public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value) where TKey : notnull
        {
            dictionary[key] = value;
        }

        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> createFunc) where TKey : notnull
        {
            if (dictionary.TryGetValue(key, out TValue? value))
                return value;
            else
            {
                var newVal = createFunc(key);
                dictionary.Add(key, newVal);
                return newVal;
            }
        }
    }

    public static class ConcurrentDictionaryExtensions
    {
        public static void AddOrUpdate<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, TValue value) where TKey : notnull
        {
            dictionary.AddOrUpdate(key, value, (key, oldV) => value);
        }

        public static void Remove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key) where TKey : notnull
        {
            dictionary.TryRemove(key, out var _);
        }
    }
}
