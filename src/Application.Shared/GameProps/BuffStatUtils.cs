using Application.Utility;

namespace Application.Shared.GameProps
{
    /// <summary>
    /// <see cref="BuffStat"/> 的辅助方法（枚举没法再挂 <c>From</c> / <c>FromBit</c> 之类的静态方法）。
    /// </summary>
    public static class BuffStatUtils
    {
        /// <summary>按名字查状态（忽略大小写，别名同样有效），名字不认识时返回 false。</summary>
        public static bool TryParse(string? name, out BuffStat stat)
        {
            stat = default;
            if (string.IsNullOrEmpty(name) || !Enum.TryParse(name, ignoreCase: true, out stat))
            {
                return false;
            }

            // Enum.TryParse 会把 "21" 这种数字串也解析成功，这里要求必须是已定义的名字
            return Enum.IsDefined(stat);
        }

        /// <summary>
        /// 按客户端掩码位还原状态（存档、协议里存的就是位号）。
        /// 同一个位有多个名字时返回枚举值本身，<c>ToString()</c> 取声明在前面的那个。
        /// </summary>
        public static BuffStat? FromBit(int bit)
        {
            return Enum.IsDefined(typeof(BuffStat), bit) ? (BuffStat)bit : null;
        }

        /// <summary>所有已定义的状态（含共用同一位的别名）。</summary>
        public static IReadOnlyList<BuffStat> Values => EnumCache<BuffStat>.GetValues();
    }
}
