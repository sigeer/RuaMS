using Application.Resources.Messages;
using server.life;

namespace Application.Core.Game.Commands.Gm2;

public class MobSkillCommand : CommandBase
{
    public MobSkillCommand() : base(2, "mobskill")
    {
    }

    public override async Task Execute(IChannelClient client, string[] paramsValue)
    {
        if (paramsValue.Length < 2)
        {
            await client.OnlinedCharacter.Yellow(nameof(ClientMessage.MobSkillCommand_Syntax));
            return;
        }

        if (!int.TryParse(paramsValue[0], out var skillId) || !int.TryParse(paramsValue[1], out var skillLevel))
        {
            await client.OnlinedCharacter.Yellow(nameof(ClientMessage.MobSkillCommand_Syntax));
            return;
        }

        var possibleSkill = MobSkillFactory.GetMobSkill(skillId, skillLevel);
        if (possibleSkill == null)
        {
            await client.OnlinedCharacter.Yellow(nameof(ClientMessage.MobSkillNotFound), skillId.ToString());
            return;
        }

        var chr = client.OnlinedCharacter;
        MobSkill mobSkill = possibleSkill;
        await chr.getMap().ProcessMonster(monster => mobSkill.applyEffect(chr, monster, false, []));
    }
}
