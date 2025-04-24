using NLua;
using System.Collections;

namespace Application.Scripting.Lua
{
    public static class LuaTableUtils
    {
        public static List<object> ToList(LuaTable table)
        {
            var list = new List<object>();
            foreach (var key in table.Keys)
            {
                list.Add(table[key]);
            }
            return list;
        }
        public static int[] ToInt32Array(LuaTable table)
        {
            int[] array = new int[table.Keys.Count];
            for (int i = 0; i < table.Keys.Count; i++)
            {
                array[i] = Convert.ToInt32(table[i + 1]);
            }
            return array;
        }

        public static IList ToListA(LuaTable table, string targetTypeName)
        {
            if (!CLRTypeManager.TypeSource.TryGetValue(targetTypeName, out var targetType) || targetType == null)
                return ToList(table);

            var listType = typeof(List<>).MakeGenericType(targetType);
            var list = (IList)Activator.CreateInstance(listType);

            foreach (var key in table.Keys)
            {
                var value = table[key];
                if (value != null && targetType.IsAssignableFrom(value.GetType()))
                {
                    list.Add(value);
                }
                else
                {
                    throw new InvalidCastException($"Element at key '{key}' cannot be cast to {targetType}.");
                }
            }

            return list;
        }
    }
}
