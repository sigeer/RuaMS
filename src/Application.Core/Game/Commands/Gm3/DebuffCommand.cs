using Application.Resources.Messages;
using server.life;

namespace Application.Core.Game.Commands.Gm3;
public class DebuffCommand : CommandBase
{
    public DebuffCommand() : base(3, "debuff")
    {
    }

    public override Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.YellowMessageI18N(nameof(ClientMessage.DebuffCommand_Syntax));
            return Task.CompletedTask;
        }

        var disease = EnumClassCache<Disease>.GetValue(paramsValue[0]);
        if (disease == null)
        {
            player.Yellow(nameof(ClientMessage.DebuffCommand_Syntax), string.Join('|', EnumClassCache<Disease>.Values.Select(x => x.name())));
            return Task.CompletedTask;
        }

        int level = -1;
        if (paramsValue.Length > 1)
            int.TryParse(paramsValue[1], out level);

        var skill = MobSkillFactory.getMobSkill(disease.getMobSkillType(), level);
        if (skill == null)
        {
            player.Yellow(nameof(ClientMessage.DebuffCommand_Syntax), string.Join('|', EnumClassCache<Disease>.Values.Select(x => x.name())));
            return Task.CompletedTask;
        }

        foreach (var mmo in player.getMap().getMapObjectsInRange(player.getPosition(), 777777.7, Arrays.asList(MapObjectType.PLAYER)))
        {
            Player chr = (Player)mmo;

            if (chr.getId() != player.getId())
            {
                chr.giveDebuff(disease, skill);
            }
        }
        return Task.CompletedTask;
    }
}
