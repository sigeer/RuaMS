using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm3;
public class WarpAreaCommand : CommandBase
{
    public WarpAreaCommand() : base(3, "warparea")
    {
    }

    public override Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.YellowMessageI18N(nameof(ClientMessage.WarpAreaCommand_Syntax));
            return Task.CompletedTask;
        }

        try
        {
            var target = c.getChannelServer().getMapFactory().getMap(int.Parse(paramsValue[0]));
            if (target == null)
            {
                player.YellowMessageI18N(nameof(ClientMessage.MapNotFound));
                return Task.CompletedTask;
            }

            Point pos = player.getPosition();

            var characters = player.getMap().getAllPlayers();

            foreach (var victim in characters)
            {
                if (victim.getPosition().distanceSq(pos) <= 50000)
                {
                    victim.saveLocationOnWarp();
                    victim.changeMap(target, target.getRandomPlayerSpawnpoint());
                }
            }
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
            player.YellowMessageI18N(nameof(ClientMessage.MapNotFound));
        }
        return Task.CompletedTask;
    }
}
