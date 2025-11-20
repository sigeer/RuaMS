using Quartz;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using YamlDotNet.Core.Tokens;

namespace Application.Utility.Extensions
{
    public static class DictionaryExtensions
    {
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

        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value) where TKey : notnull
        {
            ref TValue? v = ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary, key, out var exisits);
            if (exisits)
                return v;
            v = value;
            return v;
        }

        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TValue> valueFuc) where TKey : notnull
        {
            ref TValue? v = ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary, key, out var exisits);
            if (exisits )
                return v;
            v = valueFuc();
            return v;
        }

        public static void UpdateOnly<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value) where TKey : notnull
        {
            ref var valueRef = ref CollectionsMarshal.GetValueRefOrNullRef(dictionary, key);
            if (!Unsafe.IsNullRef(ref valueRef))
                valueRef = value;
        }

        public static void UpdateOnly<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TValue, TValue> valueFunc) where TKey : notnull
        {
            ref var valueRef = ref CollectionsMarshal.GetValueRefOrNullRef(dictionary, key);
            if (!Unsafe.IsNullRef(ref valueRef))
                valueRef = valueFunc(valueRef);
        }

        public static void Update<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TValue?, TValue> valueFunc) where TKey : notnull
        {
            ref var valueRef = ref CollectionsMarshal.GetValueRefOrNullRef(dictionary, key);
            if (!Unsafe.IsNullRef(ref valueRef))
                valueRef = valueFunc(valueRef);
            else
            {
                // 键不存在，我们使用初始值调用valueFunc，然后将结果添加到字典
                var newValue = valueFunc(default(TValue));
                dictionary.Add(key, newValue);
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
