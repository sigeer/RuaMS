namespace Application.Templates.Skill
{
    public sealed class SkillTemplate : AbstractTemplate
    {
        public SkillTemplate(int nSkillID) : base(nSkillID)
        {
            EffectData = Array.Empty<SkillEffectData>();
            LevelData = Array.Empty<SkillLevelData>();
        }

        public string? ElemAttr { get; set; }
        public int MaxLevel { get; set; }
        public int SkillType { get; set; } = -1;
        public bool HasHitNode { get; set; }
        public bool HasBallNode { get; set; }
        [WZPath("skill/xxx/action/0")]
        public string? Action0 { get; set; }
        [WZPath("skill/xxx/prepare/action")]
        public string? PrepareAction { get; set; }
        public SkillEffectData[] EffectData { get; set; }
        [WZPath("skill/xxx/action/level")]
        public SkillLevelData[] LevelData { get; set; }
    }
}
