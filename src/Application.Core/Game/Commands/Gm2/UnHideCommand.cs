using Application.Core.Game.Skills;

namespace Application.Core.Game.Commands.Gm2;
public class UnHideCommand : CommandBase
{
    public UnHideCommand() : base(2, "unhide")
    {
        Description = "Toggle Hide.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        var hideSkill = SkillFactory.GetSkillTrust(SuperGM.HIDE);
        hideSkill.getEffect(hideSkill.getMaxLevel()).applyTo(player);

    }
}
