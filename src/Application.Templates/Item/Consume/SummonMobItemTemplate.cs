namespace Application.Templates.Item.Consume
{
    /// <summary>
    /// 210
    /// </summary>
    [GenerateTag]
    public sealed class SummonMobItemTemplate : ConsumeItemTemplate
    {
        public SummonMobItemTemplate(int templateId) : base(templateId)
        {
            SummonData = Array.Empty<SummonData>();
        }
        [WZPath("mob/-")]
        public SummonData[] SummonData { get; set; }
    }

    public class SummonData
    {
        [WZPath("mob/-/id")]
        public int Mob { get; set; }
        public int Prob { get; set; }
    }
}
