using Application.Core.Server.life;
using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm3;

public class DebuffCommand : CommandBase
{
    public DebuffCommand() : base(3, "debuff")
    {
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            await player.Yellow(nameof(ClientMessage.DebuffCommand_Syntax));
            return;
        }

        if (!BuffStatUtils.TryParse(paramsValue[0], out BuffStat disease) || !DiseaseInfo.IsDisease(disease))
        {
            await player.Yellow(nameof(ClientMessage.DebuffCommand_Syntax), string.Join('|', DiseaseInfo.Diseases));
            return;
        }

        int level = -1;
        if (paramsValue.Length > 1)
            int.TryParse(paramsValue[1], out level);

        var mobSkillType = DiseaseInfo.GetMobSkillType(disease);
        if (mobSkillType == null)
        {
            await player.Yellow(nameof(ClientMessage.DebuffCommand_Syntax), string.Join('|', DiseaseInfo.Diseases));
            return;
        }

        var skill = MobSkillFactory.getMobSkill(mobSkillType.Value, level);
        if (skill == null)
        {
            await player.Yellow(nameof(ClientMessage.DebuffCommand_Syntax), string.Join('|', DiseaseInfo.Diseases));
            return;
        }

        foreach (var mmo in player.getMap().getMapObjectsInRange(player.getPosition(), 777777.7, [MapObjectType.PLAYER]))
        {
            Player chr = (Player)mmo;

            await chr.giveDebuff(disease, skill);
        }
    }
}
