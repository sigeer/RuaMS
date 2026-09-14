using Application.Templates.Skill;

namespace Application.Shared.MapObjects.Summons
{
    public enum SummonAssistantType : byte
    {
        None = 0,
        Attack = 1,
        HealOrBuff = 2,
    }

    public static class SummonAssistantTypeExtensions
    {
        public static SummonAssistantType GetSummonAssistantType(this SkillSummonData template)
        {
            if (template.CanAttack)
            {
                return SummonAssistantType.Attack;
            }
            else if (template.CanSkill)
            {
                return SummonAssistantType.HealOrBuff;
            }
            else
            {
                return SummonAssistantType.None;
            }
        }
    }
}
