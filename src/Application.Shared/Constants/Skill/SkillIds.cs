namespace Application.Shared.Constants.Skill
{
    public class SkillIds
    {
        public static int[] GuardSkills = new int[] { Hero.GUARDIAN, Paladin.GUARDIAN };
        public static int[] ShadowShifter = [Shadower.SHADOW_SHIFTER, NightLord.SHADOW_SHIFTER];
        public static int[] AllGuardSkills = [.. GuardSkills, .. ShadowShifter];
        public static int[] AchillesSkills = [Hero.ACHILLES, DarkKnight.ACHILLES, Paladin.ACHILLES];

        /// <summary>
        /// 是被动技能
        /// </summary>
        /// <param name="skillId"></param>
        /// <returns></returns>
        public static bool IsPassiveSkill(int skillId)
        {
            return (skillId / 1000) % 10 == 0;
        }
    }
}
