using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Utility
{
    public static class KeyValueStringParser
    {
        /// <summary>
        /// 将格式 "key1=value1;key2=value2" 解析为字典。
        /// 支持值中包含等号（第一个等号作为分隔符），但值中不能包含分号（分隔符）。
        /// 默认去除键和值的首尾空白字符。
        /// </summary>
        public static Dictionary<string, string> Parse(ReadOnlySpan<char> input)
        {
            var dict = new Dictionary<string, string>();
            var remaining = input;

            while (!remaining.IsEmpty)
            {
                // 查找分号位置
                int semicolonIdx = remaining.IndexOf(';');
                ReadOnlySpan<char> pairSpan = semicolonIdx == -1
                    ? remaining
                    : remaining.Slice(0, semicolonIdx);

                // 处理单个键值对
                if (!pairSpan.IsEmpty)
                {
                    int eqIdx = pairSpan.IndexOf('=');
                    if (eqIdx > 0) // 必须包含等号且键非空
                    {
                        var key = pairSpan.Slice(0, eqIdx).Trim();
                        var value = pairSpan.Slice(eqIdx + 1).Trim();
                        if (!key.IsEmpty)
                            dict[key.ToString()] = value.ToString();
                    }
                    // 忽略格式错误的项（如没有等号或键为空）
                }

                // 移动到下一个
                if (semicolonIdx == -1) break;
                remaining = remaining.Slice(semicolonIdx + 1);
            }

            return dict;
        }

        /// <summary>
        /// 从键值对生成 "key1=value1;key2=value2" 格式的字符串。
        /// 值中的分号会被转义（替换为 \;），以避免解析冲突。
        /// </summary>
        public static string Build(IEnumerable<KeyValuePair<string, string>> pairs)
        {
            var sb = new StringBuilder();
            bool first = true;
            foreach (var kv in pairs)
            {
                if (!first) sb.Append(';');
                first = false;

                // 键中不应包含 = 或 ;，但为了健壮，可以对键也做转义，不过通常键是安全的
                sb.Append(kv.Key.Trim());
                sb.Append('=');
                // 转义值中的分号
                var escapedValue = kv.Value?.Replace(";", @"\;") ?? "";
                sb.Append(escapedValue);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 尝试从输入中获取指定键的值（无需完整解析所有键值对）。
        /// 适用于只关心一个或少数键的场景，性能更高。
        /// </summary>
        public static bool TryGetValue(ReadOnlySpan<char> input, ReadOnlySpan<char> key, out string? value)
        {
            value = null;
            var remaining = input;
            while (!remaining.IsEmpty)
            {
                int semicolonIdx = remaining.IndexOf(';');
                ReadOnlySpan<char> pairSpan = semicolonIdx == -1
                    ? remaining
                    : remaining.Slice(0, semicolonIdx);

                if (!pairSpan.IsEmpty)
                {
                    int eqIdx = pairSpan.IndexOf('=');
                    if (eqIdx > 0)
                    {
                        var currentKey = pairSpan.Slice(0, eqIdx).Trim();
                        if (currentKey.Equals(key, StringComparison.Ordinal))
                        {
                            var val = pairSpan.Slice(eqIdx + 1).Trim();
                            value = val.ToString();
                            return true;
                        }
                    }
                }

                if (semicolonIdx == -1) break;
                remaining = remaining.Slice(semicolonIdx + 1);
            }
            return false;
        }
    }
}
