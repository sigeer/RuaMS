using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Application.Core.EF.Utils
{
    public class DateTimeOffsetToUnixMillisecondsConverter : ValueConverter<DateTimeOffset, long>
    {
        public DateTimeOffsetToUnixMillisecondsConverter()
            : base(
                v => v.ToUnixTimeMilliseconds(),               // 写入：转为毫秒时间戳
                v => DateTimeOffset.FromUnixTimeMilliseconds(v) // 读取：转回 DateTimeOffset
            )
        { }
    }


    public class DateTimeOffsetToUnixMillisecondsNullableConverter : ValueConverter<DateTimeOffset?, long?>
    {
        public DateTimeOffsetToUnixMillisecondsNullableConverter()
            : base(
                v => v.HasValue ? v.Value.ToUnixTimeMilliseconds() : (long?)null,
                v => v.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(v.Value) : (DateTimeOffset?)null
            )
        { }
    }
}
