using Application.Core.Game.Skills;
using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm2;

public class MaxSkillCommand : CommandBase
{
    public MaxSkillCommand() : base(2, "maxskill")
    {
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        foreach (var skillId in c.CurrentCulture.StringProvider.GetSubProvider(Templates.String.StringCategory.Skill).LoadAll().Select(x => x.TemplateId))
        {
            try
            {
                Skill skill = SkillFactory.GetSkillTrust(skillId);
                player.changeSkillLevel(skill, (sbyte)skill.getMaxLevel(), skill.getMaxLevel(), -1);
            }
            catch (Exception nfe)
            {
                log.Error(nfe.ToString());
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

        player.YellowMessageI18N(nameof(ClientMessage.MaxSkillCommand_Result));
    }
}
