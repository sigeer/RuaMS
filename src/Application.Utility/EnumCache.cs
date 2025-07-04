namespace Application.Utility
{
    public static class EnumCache<T> where T : Enum
    {
        // 缓存所有枚举值
        public static readonly T[] Values;

        static EnumCache()
        {
            Values = (T[])Enum.GetValues(typeof(T));
        }

        /// <summary>
        /// 判断是否是合法枚举值
        /// </summary>
        public static bool IsDefined(T value) => Values.Contains(value);

        /// <summary>
        /// 获取枚举总数
        /// </summary>
        public static int Count => Values.Length;

        /// <summary>
        /// 获取所有枚举值（只读数组）
        /// </summary>
        public static IReadOnlyList<T> GetValues() => Values;

    }

    public static class EnumClassCache<T> where T : EnumClass
    {
        // 缓存所有枚举值
        public static readonly T[] Values;

        static EnumClassCache()
        {
            Values = EnumClassUtils.GetValues<T>().ToArray();
        }

        /// <summary>
        /// 判断是否是合法枚举值
        /// </summary>
        public static bool IsDefined(T value) => Values.Contains(value);

        /// <summary>
        /// 获取枚举总数
        /// </summary>
        public static int Count => Values.Length;

        /// <summary>
        /// 获取所有枚举值（只读数组）
        /// </summary>
        public static IReadOnlyList<T> GetValues() => Values;
    }

}
