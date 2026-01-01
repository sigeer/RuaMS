using Application.Core.Game.Skills;

namespace Application.Core.Game.Commands.Gm2;
public class ResetSkillCommand : CommandBase
{
    public ResetSkillCommand() : base(2, "resetskill")
    {
        Description = "Set all skill levels to 0.";
    }

    public override Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        foreach (var skillId in c.CurrentCulture.StringProvider.GetSubProvider(Templates.String.StringCategory.Skill).LoadAll().Select(x => x.TemplateId))
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
        return Task.CompletedTask;
    }
}
