namespace Application.Templates.Item.Consume
{
    /// <summary>
    /// 227
    /// </summary>
    [GenerateTag]
    public sealed class CatchMobItemTemplate : ConsumeItemTemplate
    {
        public CatchMobItemTemplate(int templateId) : base(templateId)
        {
        }

        [WZPath("info/mobHP")]
        public int MobHP { get; set; }
        [WZPath("info/mob")]
        public int Mob { get; set; }
        [WZPath("info/useDelay")]
        public int UseDelay { get; set; }
        [WZPath("info/create")]
        public int Create { get; set; }
        [WZPath("info/useDelay")]
        public string? DelayMsg { get; set; }
        [WZPath("info/bridleProp")]
        public int BridleProp { get; set; }
    }
}
