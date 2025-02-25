using Application.Core.Game.Skills;
using constants.skills;

namespace Application.Core.Game.Commands.Gm2;

public class EmpowerMeCommand : CommandBase
{
    public EmpowerMeCommand() : base(2, "empowerme")
    {
        Description = "Activate all useful buffs.";
    }

    public override void Execute(IClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        int[] array = { 
            Priest.HOLY_SYMBOL, 
            Cleric.BLESS, 
            Spearman.HYPER_BODY, 
            Assassin.HASTE, 
            Magician.MAGIC_GUARD, 
            Fighter.POWER_GUARD, 
            Beginner.ECHO_OF_HERO, 
            Cleric.INVINCIBLE, 
            Buccaneer.SPEED_INFUSION, 
            Crusader.COMBO, 
            Hermit.MESO_UP, 
            Hermit.SHADOW_PARTNER, 
            ChiefBandit.PICKPOCKET, 
            ChiefBandit.MESO_GUARD, 
            DarkKnight.MAPLE_WARRIOR, 
            Bishop.INFINITY, 
            Bowmaster.SHARP_EYES };
        foreach (int i in array)
        {
            var skill = SkillFactory.GetSkillTrust(i);
            skill.getEffect(skill.getMaxLevel()).applyTo(player);
        }
    }
}
