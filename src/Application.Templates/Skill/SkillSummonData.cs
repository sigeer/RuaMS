namespace Application.Templates.Skill
{
    [GenerateTag]
    public sealed class SkillSummonData
    {
        [WZPath("~/attack1/$existed")]
        public bool CanAttack { get; set; }
        [WZPath("~/skill1/$existed")]
        public bool CanSkill { get; set; }
        [WZPath("~/move/$existed")]
        public bool CanMove { get; set; }
        [WZPath("~/fly/$existed")]
        public bool CanFly { get; set; }
    }
}
