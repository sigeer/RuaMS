using Application.Core.Game.Skills;
using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm2;

public class BuffCommand : CommandBase
{
    public BuffCommand() : base(2, "buff")
    {
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.YellowMessageI18N(nameof(ClientMessage.BuffCommand_Syntax));
            return;
        }
        int skillid = int.Parse(paramsValue[0]);

        var skill = SkillFactory.getSkill(skillid);
        if (skill != null)
        {
            skill.getEffect(skill.getMaxLevel()).applyTo(player);
        }
    }
}
