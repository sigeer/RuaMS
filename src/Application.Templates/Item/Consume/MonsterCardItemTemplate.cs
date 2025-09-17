namespace Application.Templates.Item.Consume
{
    /// <summary>
    /// 238
    /// </summary>
    [GenerateTag]
    public class MonsterCardItemTemplate : PotionItemTemplate, IMesoUpEffect, IItemUpEffect, IMapProtectEffect
    {
        public MonsterCardItemTemplate(int templateId) : base(templateId)
        {
            Con = Array.Empty<ConData>();
        }

        [WZPath("info/mob")]
        public int MobId { get; set; }

        [WZPath("spec/itemCode")]
        public int ItemCode { get; set; } = 1;
        [WZPath("spec/itemRange")]
        public int ItemRange { get; set; } = 1;
        [WZPath("spec/respectPimmune")]
        public bool RespectPimmune { get; set; }
        [WZPath("spec/respectMimmune")]
        public bool RespectMimmune { get; set; }
        [WZPath("spec/defenseAtt")]
        public string? DefenseAtt { get; set; }
        [WZPath("spec/defenseState")]
        public string? DefenseState { get; set; }

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
        public int Type { get; set; } = -1;
        [WZPath("spec/con/-/inParty")]
        public bool InParty { get; set; }
    }
}
