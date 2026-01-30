using Application.Core.Game.Skills;

namespace Application.Core.Game.Commands.Gm2;

public class HideCommand : CommandBase
{
    public HideCommand() : base(2, "hide")
    {
        Description = "Hide from players.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        var hideSkill = SkillFactory.GetSkillTrust(SuperGM.HIDE);
        hideSkill.getEffect(hideSkill.getMaxLevel()).applyTo(player);
    }
}
