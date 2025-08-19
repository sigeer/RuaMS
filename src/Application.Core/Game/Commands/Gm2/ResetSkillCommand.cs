using Application.Core.Game.Skills;
using Application.Resources;

namespace Application.Core.Game.Commands.Gm2;
public class ResetSkillCommand : CommandBase
{
    readonly WzStringProvider _wzStringProvider;
    public ResetSkillCommand(WzStringProvider wzStringProvider) : base(2, "resetskill")
    {
        _wzStringProvider = wzStringProvider;
        Description = "Set all skill levels to 0.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        foreach (var skillId in _wzStringProvider.GetAllSkillIdList())
        {
            try
            {
                var skill = SkillFactory.GetSkillTrust(skillId);
                player.changeSkillLevel(skill, 0, skill.getMaxLevel(), -1);
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                break;
            }
        }

        if (player.getJob().isA(Job.ARAN1) || player.getJob().isA(Job.LEGEND))
        {
            Skill skill = SkillFactory.GetSkillTrust(5001005);
            player.changeSkillLevel(skill, -1, -1, -1);
        }
        else
        {
            Skill skill = SkillFactory.GetSkillTrust(21001001);
            player.changeSkillLevel(skill, -1, -1, -1);
        }

        player.yellowMessage("Skills reseted.");
    }
}
