namespace Application.Templates.Item.Consume
{
    /// <summary>
    /// 238
    /// </summary>
    [GenerateTag]
    public class MonsterCardItemTemplate : ConsumeItemTemplate
    {
        public MonsterCardItemTemplate(int templateId) : base(templateId)
        {
            Con = Array.Empty<ConData>();
        }

        [WZPath("info/mob")]
        public int MobId { get; set; }

        [WZPath("spec/itemupbyitem")]
        public bool ItemUpByItem { get; set; }
        [WZPath("spec/itemCode")]
        public int ItemCode { get; set; }
        [WZPath("spec/con/-")]
        public ConData[] Con { get; set; }
    }

    public sealed class ConData
    {
        [WZPath("spec/con/-/sMap")]
        public int StartMap { get; set; }
        [WZPath("spec/con/-/eMap")]
        public int EndMap { get; set; }
        [WZPath("spec/con/-/type")]
        public int Type { get; set; }
        [WZPath("spec/con/-/inParty")]
        public bool InParty { get; set; }
    }
}
