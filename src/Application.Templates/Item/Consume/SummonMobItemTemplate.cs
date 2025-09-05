namespace Application.Templates.Item.Consume
{
    /// <summary>
    /// 210
    /// </summary>
    public sealed class SummonMobItemTemplate : AbstractItemTemplate
    {
        public SummonMobItemTemplate(int templateId) : base(templateId)
        {
            SummonData = Array.Empty<SummonData>();
        }

        public SummonData[] SummonData { get; set; }
    }

    public class SummonData
    {
        public int Mob { get; set; }
        public int Prop { get; set; }
    }
}
