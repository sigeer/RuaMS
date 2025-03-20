namespace Application.Scripting.JS
{
    public static class JsEngineExtensions
    {
        /// <summary>
        /// 兼容js
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static int Size(this object list)
        {
            var objType = list.GetType();
            if (objType.IsGenericType && objType.GetGenericTypeDefinition() == typeof(List<>))
                return (int)objType.GetProperty("Count")!.GetValue(list)!;
            if (objType.IsArray && list is Array arr)
                return arr.Length;
            return 0;
        }

        public static object? Get(this object list, int index)
        {
            var objType = list.GetType();
            if (objType.IsGenericType && objType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var indexer = objType.GetProperty("Item");
                if (indexer != null)
                {
                    var parameters = indexer.GetIndexParameters();
                    if (parameters.Length > 0 && parameters[0].ParameterType == typeof(int))
                    {
                        return indexer.GetValue(list, new object[] { index });
                    }
                }
            }
            if (objType.IsArray && list is Array arr)
            {
                return arr.GetValue(index);
            }
            return null;
        }


        public static bool IsEmpty(this object list)
        {
            return list.Size() == 0;
        }

        public static int GetId(this object m)
        {
            if (m is IConvertible)
                return Convert.ToInt32(m);
            return 0;
        }
    }
}
