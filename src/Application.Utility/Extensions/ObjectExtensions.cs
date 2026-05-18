using System.Reflection;

namespace Application.Utility.Extensions
{
    public static class ObjectExtensions
    {
        public static sbyte[] ToSBytes(this byte[] byteArray)
        {
            var dp = new sbyte[byteArray.Length];
            for (int i = 0; i < byteArray.Length; i++)
            {
                dp[i] = (sbyte)byteArray[i];
            }
            return dp;
        }

        public static byte[] ToBytes(this sbyte[] byteArray)
        {
            var dp = new byte[byteArray.Length];
            for (int i = 0; i < byteArray.Length; i++)
            {
                dp[i] = (byte)byteArray[i];
            }
            return dp;
        }

        public static string AdpteSP(this int[] sps)
        {
            return string.Join(',', sps);
        }

        public static int[] AdpteSP(this string sp)
        {
            return sp.Split(',').Select(int.Parse).ToArray();
        }


        public static T DeepCopyByReflection<T>(this T obj)
        {
            if (obj == null) return default;

            Type type = obj.GetType();

            // 如果是值类型或字符串，直接返回
            if (type.IsValueType || type == typeof(string))
                return obj;

            // 处理数组
            if (type.IsArray)
            {
                Type elemType = type.GetElementType();
                Array sourceArray = obj as Array;
                Array destArray = Array.CreateInstance(elemType, sourceArray.Length);
                for (int i = 0; i < sourceArray.Length; i++)
                {
                    destArray.SetValue(DeepCopyByReflection(sourceArray.GetValue(i)), i);
                }
                return (T)(object)destArray;
            }

            // 处理集合（ICollection<T>、IList 等可简化，这里以 List<T> 为例）
            if (obj is System.Collections.IEnumerable enumerable && !(obj is string))
            {
                var listType = typeof(List<>);
                var itemType = type.GetGenericArguments()[0];
                var list = Activator.CreateInstance(listType.MakeGenericType(itemType)) as System.Collections.IList;
                foreach (var item in enumerable)
                {
                    list.Add(DeepCopyByReflection(item));
                }
                return (T)list;
            }

            // 普通对象：创建实例并复制所有字段（public + nonpublic instance）
            object result = Activator.CreateInstance(type);
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            FieldInfo[] fields = type.GetFields(flags);
            foreach (var field in fields)
            {
                object fieldValue = field.GetValue(obj);
                field.SetValue(result, DeepCopyByReflection(fieldValue));
            }

            PropertyInfo[] properties = type.GetProperties(flags);
            foreach (var prop in properties)
            {
                if (prop.CanWrite && prop.GetIndexParameters().Length == 0) // 忽略索引器
                {
                    object propValue = prop.GetValue(obj);
                    prop.SetValue(result, DeepCopyByReflection(propValue));
                }
            }
            return (T)result;
        }
    }
}

