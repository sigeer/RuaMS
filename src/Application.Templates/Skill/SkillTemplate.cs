using Application.Templates.StatEffectProps;

namespace Application.Templates.Skill
{
    [GenerateTag]
    public sealed class SkillTemplate : AbstractTemplate, IStatEffectSource
    {
        public SkillTemplate(int nSkillID) : base(nSkillID)
        {
            LevelData = Array.Empty<SkillLevelData>();
        }
        [GenerateIgnoreProperty]
        public int SourceId => TemplateId;

        [WZPath("elemAttr")]
        public string? ElemAttr { get; set; }
        [WZPath("masterLevel")]
        public int MasterLevel { get; set; }
        public int SkillType { get; set; } = -1;
        [WZPath("hit/$existed")]
        public bool HasHitNode { get; set; }
        [WZPath("ball/$existed")]
        public bool HasBallNode { get; set; }
        [WZPath("prepare/action")]
        public string? PrepareAction { get; set; }
        [WZPath("effect/-")]
        public SkillEffectData[]? EffectData { get; set; }
        [WZPath("level/-")]
        public SkillLevelData[] LevelData { get; set; }
        [WZPath("action")]
        public SkillActionData? ActionData { get; set; }
        [WZPath("summon/$existed")]
        public bool HasSummonNode { get; set; }
    }
}
