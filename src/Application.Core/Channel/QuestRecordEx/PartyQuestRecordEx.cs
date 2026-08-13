namespace Application.Core.Channel.QuestRecordEx
{
    public class PartyQuestRecordEx : AbstractQuestRecordEx
    {
        /// <summary>
        /// 当前评分
        /// </summary>
        [QuestRecordExKey("rank")]
        public string Rank { get; set; } = "F";
        /// <summary>
        /// 完成次数
        /// </summary>
        [QuestRecordExKey("cmp")]
        public int Cmp { get; set; }
        /// <summary>
        /// 参与次数
        /// </summary>
        [QuestRecordExKey("try")]
        public int Try { get; set; }

        /// <summary>
        /// 最短用时
        /// </summary>
        public long TotalCost { get; set; }
        /// <summary>
        /// 最短用时完成时间
        /// </summary>
        public long CompleteTime { get; set; }

        public PartyQuestRecordEx(short questId, string? rawContent) : base(questId, rawContent)
        {
        }
        public PartyQuestRecordEx(short questId) : this(questId, null)
        {
        }

        protected override IEnumerable<string> GenerateData()
        {
            List<string> arr = [];
            arr.AddRange(base.GenerateData());
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
                arr.Add($"date={DateTimeOffset.FromUnixTimeMilliseconds(CompleteTime).ToLocalTime():yyyy-MM-dd HH:mm:ss}");
            return arr;
        }
    }

    ///// <summary>
    ///// 竞技类型组队任务
    ///// </summary>
    //public class ConfrontQuestEx
    //{
    //    public int Try { get; set; }
    //    public int VicCount { get; set; }
    //    public int LoseCount { get; set; }
    //    public int DrawCount { get; set; }
    //    public int GiveUpCount { get; set; }

    //    public ConfrontQuestEx(string rawContent)
    //    {
    //        var dic = KeyValueStringParser.Parse(rawContent);
    //        Try = GetInt(dic, "try");
    //        VicCount = GetInt(dic, "vic");
    //        DrawCount = GetInt(dic, "draw");
    //        LoseCount = GetInt(dic, "lose");
    //        GiveUpCount = GetInt(dic, "gvup");
    //    }

    //    static int GetInt(IReadOnlyDictionary<string, string> dic, string key)
    //    {
    //        return dic.TryGetValue(key, out var value) && int.TryParse(value, out var result) ? result : 0;
    //    }
    //    public ConfrontQuestEx()
    //    {

    //    }
    //}
}
