using Application.Templates.StatEffectProps;

namespace Application.Templates.Item.Cash
{
    /// <summary>
    /// 521, 536
    /// </summary>
    [GenerateTag]
    public class CouponItemTemplate : EffectCashItemTemplate, IItemStatEffectProp
    {
        public CouponItemTemplate(int templateId) : base(templateId)
        {
            TimeRange = Array.Empty<string>();
        }

        [WZPath("info/rate")]
        public float Rate { get; set; }

        [WZPath("info/time/-")]
        public string[] TimeRange { get; set; }
        [WZPath("spec/expR")]
        public bool IsExp { get; set; }
        [WZPath("spec/drpR")]
        public bool IsDrop { get; set; }

        WeeklyTimeRange[]? _data;
        [GenerateIgnoreProperty]
        public WeeklyTimeRange[] TimeRangeF => _data ??= TimeRange.Select(WeeklyTimeRange.Parse).ToArray();
    }

    public class WeeklyTimeRange
    {
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }   // 如 06:00
        public TimeOnly EndTime { get; set; }     // 如 10:00
        bool crossesMidnight { get; set; }
        public bool Contains(DateTime moment)
        {
            // 先检查星期几是否匹配
            if (moment.DayOfWeek != DayOfWeek)
                return false;

            // 再检查时间部分是否在 [StartTime, EndTime] 区间
            var time = TimeOnly.FromDateTime(moment);
            return time >= StartTime && time <= EndTime;
        }

        public static WeeklyTimeRange Parse(string input)
        {
            var parts = input.Split(':');
            if (parts.Length != 2)
                throw new FormatException("格式应为 'DOW:HH-HH'");

            var dayStr = parts[0].ToUpper();
            var dayOfWeek = dayStr switch
            {
                "MON" => DayOfWeek.Monday,
                "TUE" => DayOfWeek.Tuesday,
                "WED" => DayOfWeek.Wednesday,
                "THU" => DayOfWeek.Thursday,
                "FRI" => DayOfWeek.Friday,
                "SAT" => DayOfWeek.Saturday,
                "SUN" => DayOfWeek.Sunday,
                _ => throw new ArgumentException($"未知星期: {dayStr}")
            };

            var hourRange = parts[1].Split('-');
            if (hourRange.Length != 2)
                throw new FormatException("小时范围应为 'HH-HH'");

            int startHour = int.Parse(hourRange[0]);
            int endHour = int.Parse(hourRange[1]);

            // 创建 TimeOnly（默认分钟、秒为0）
            var startTime = new TimeOnly(startHour, 0);
            TimeOnly endTime;
            if (endHour == 24)
            {
                endTime = new TimeOnly(23, 59, 59);
            }
            else
            {
                // 结束时间：指定小时的 59 分 59 秒，确保覆盖整个小时区间
                endTime = new TimeOnly(endHour - 1, 59, 59);
            }

            return new WeeklyTimeRange
            {
                DayOfWeek = dayOfWeek,
                StartTime = startTime,
                EndTime = endTime
            };
        }
    }
}
