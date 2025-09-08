namespace Application.Templates.Item.Consume
{
    /// <summary>
    /// 228.技能册，229.能手册，以<see cref="SuccessRate"/>的概率，将等级上限达到<see cref="ReqSkillLevel"/>的技能<see cref="Skills"/>提升到<see cref="MasterLevel"/>
    /// </summary>
    [GenerateTag]
    public sealed class MasteryItemTemplate : ConsumeItemTemplate
    {
        public MasteryItemTemplate(int templateId) : base(templateId)
        {
            Skills = Array.Empty<int>();
        }

        [WZPath("info/masterLevel")]
        public int MasterLevel { get; set; }

        [WZPath("info/reqSkillLevel")]
        public int ReqSkillLevel { get; set; }
        [WZPath("info/success")]
        public int SuccessRate { get; set; }
        [WZPath("info/skill/-")]
        public int[] Skills { get; set; }
    }
}
