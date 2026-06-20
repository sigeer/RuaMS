using Application.Core.Game.Skills;

namespace Application.Core.Game.Commands.Gm1;

public class BuffMeCommand : CommandBase
{
    public BuffMeCommand() : base(1, "buffme")
    {
        Description = "Activate GM buffs on self.";
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;

        await SkillFactory.GetSkillTrust(4101004).getEffect(SkillFactory.GetSkillTrust(4101004).getMaxLevel()).applyTo(player);
        await SkillFactory.GetSkillTrust(2311003).getEffect(SkillFactory.GetSkillTrust(2311003).getMaxLevel()).applyTo(player);
        await SkillFactory.GetSkillTrust(1301007).getEffect(SkillFactory.GetSkillTrust(1301007).getMaxLevel()).applyTo(player);
        await SkillFactory.GetSkillTrust(2301004).getEffect(SkillFactory.GetSkillTrust(2301004).getMaxLevel()).applyTo(player);
        await SkillFactory.GetSkillTrust(1005).getEffect(SkillFactory.GetSkillTrust(1005).getMaxLevel()).applyTo(player);
        await player.healHpMp();
    }
}
