using NLua;

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
    }
}
