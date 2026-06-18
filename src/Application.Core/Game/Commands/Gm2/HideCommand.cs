using Application.Core.Game.Skills;

namespace Application.Core.Game.Commands.Gm2;

public class HideCommand : CommandBase
{
    public HideCommand() : base(2, "hide")
    {
        Description = "Hide from players.";
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        var hideSkill = SkillFactory.GetSkillTrust(SuperGM.HIDE);
        await hideSkill.getEffect(hideSkill.getMaxLevel()).applyTo(player);
    }
}
