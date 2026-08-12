using Application.Core.Channel.Net.Packets;

namespace Application.Core.Channel.QuestRecordEx
{
    public class PartyQuestRecordEx: IQuestRecordEx
    {
        public short QuestId { get; }
        /// <summary>
        /// 当前评分
        /// </summary>
        public string Rank { get; set; } = "F";
        /// <summary>
        /// 完成次数
        /// </summary>
        public int Cmp { get; set; }
        /// <summary>
        /// 参与次数
        /// </summary>
        public int Try { get; set; }

        /// <summary>
        /// 最短用时
        /// </summary>
        public long TotalCost { get; set; }
        /// <summary>
        /// 最短用时完成时间
        /// </summary>
        public long CompleteTime { get; set; }

        public PartyQuestRecordEx(short questId, string rawContent) : this(questId)
        {
            var dic = KeyValueStringParser.Parse(rawContent);
            Try = int.Parse(dic.GetValueOrDefault("Try", "0"));
            Cmp = int.Parse(dic.GetValueOrDefault("Cmp", "0"));
            Rank = dic.GetValueOrDefault("Rank") ?? Rank;
            CompleteTime = long.Parse(dic.GetValueOrDefault("CompleteTime", "0"));
            TotalCost = long.Parse(dic.GetValueOrDefault("TotalCost", "0"));
        }
        public PartyQuestRecordEx(short questId)
        {
            QuestId = questId;
        }

        public override string ToString()
        {
            List<string> arr = [];
            if (TotalCost > 0)
            {
                var ts = TimeSpan.FromMilliseconds(TotalCost);
                var mins = ts.Minutes;
                if (mins > 0)
                    arr.Add($"min={mins}");
                var secs = ts.Seconds;
                if (secs > 0)
                    arr.Add($"sec={secs}");
            }
            if (CompleteTime > 0)
                arr.Add($"date={DateTimeOffset.FromUnixTimeMilliseconds(CompleteTime).ToLocalTime()}");
            if (Cmp > 0)
                arr.Add($"cmp={Cmp}");

            return string.Join(';', arr);
        }

        public async Task Flush(Player chr)
        {
            var value = ToString();
            chr.AreaInfo[QuestId] = value;
            await chr.SendPacket(MessagePacket.QuestRecordEx(QuestId, value));
        }
    }

    /// <summary>
    /// 竞技类型组队任务
    /// </summary>
    public class ConfrontQuestEx
    {
        public int Try { get; set; }
        public int VicCount { get; set; }
        public int LoseCount { get; set; }
        public int DrawCount { get; set; }
        public int GiveUpCount { get; set; }

        public ConfrontQuestEx(string rawContent)
        {
            var dic = KeyValueStringParser.Parse(rawContent);
            Try = int.Parse(dic["try"]);
            VicCount = int.Parse(dic["vic"]);
            DrawCount = int.Parse(dic["draw"]);
            LoseCount = int.Parse(dic["lose"]);
            GiveUpCount = int.Parse(dic["gvup"]);
        }
        public ConfrontQuestEx()
        {

        }
        public override string ToString()
        {
            List<string> arr = [];
            if (Try > 0)
                arr.Add($"try={Try}");
            if (VicCount > 0)
                arr.Add($"vic={VicCount}");
            if (DrawCount > 0)
                arr.Add($"draw={DrawCount}");
            if (LoseCount > 0)
                arr.Add($"lose={LoseCount}");
            if (GiveUpCount > 0)
                arr.Add($"gvup={GiveUpCount}");

            return string.Join(';', arr);
        }
    }
}
