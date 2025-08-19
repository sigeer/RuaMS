using server.life;
using tools;

namespace Application.Core.Game.Commands.Gm2;

public class BombCommand : CommandBase
{
    public BombCommand() : base(2, "bomb")
    {
        Description = "Bomb a player, dealing damage.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length > 0)
        {
            var victim = c.getChannelServer().getPlayerStorage().getCharacterByName(paramsValue[0]);
            if (victim != null && victim.IsOnlined)
            {
                victim.getMap().spawnMonsterOnGroundBelow(LifeFactory.Instance.getMonster(MobId.ARPQ_BOMB), victim.getPosition());
                c.CurrentServerContainer.SendBroadcastWorldGMPacket(PacketCreator.serverNotice(5, player.getName() + " used !bomb on " + victim.getName()));
            }
            else
            {
                player.message("Player '" + paramsValue[0] + "' could not be found on this channel.");
            }
        }
        else
        {
            player.getMap().spawnMonsterOnGroundBelow(LifeFactory.Instance.getMonster(MobId.ARPQ_BOMB), player.getPosition());
        }
    }
}
