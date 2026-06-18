using server.life;

namespace Application.Core.Game.Commands.Gm2;

public class BombCommand : CommandBase
{
    public BombCommand() : base(2, "bomb")
    {
        Description = "Bomb a player, dealing damage.";
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length > 0)
        {
            var victim = c.getChannelServer().getPlayerStorage().getCharacterByName(paramsValue[0]);
            if (victim != null && victim.IsOnlined)
            {
                await victim.getMap().spawnMonsterOnGroundBelow(LifeFactory.Instance.GetMonsterTrust(MobId.ARPQ_BOMB), victim.getPosition());
                c.CurrentServer.NodeService.SendDropMessage(5, player.getName() + " used !bomb on " + victim.getName(), true);
            }
            else
            {
                await player.Pink("Player '" + paramsValue[0] + "' could not be found on this channel.");
            }
        }
        else
        {
            await player.getMap().spawnMonsterOnGroundBelow(LifeFactory.Instance.GetMonsterTrust(MobId.ARPQ_BOMB), player.getPosition());
        }
    }
}
