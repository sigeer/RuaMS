using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm3;

public class WarpAreaCommand : CommandBase
{
    public WarpAreaCommand() : base(3, "warparea")
    {
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            await player.Yellow(nameof(ClientMessage.WarpAreaCommand_Syntax));
            return;
        }

        try
        {
            var target = await c.getChannelServer().getMapFactory().getMap(int.Parse(paramsValue[0]));
            if (target == null)
            {
                await player.Yellow(nameof(ClientMessage.MapNotFound));
                return;
            }

            Point pos = player.getPosition();

            var characters = player.getMap().getAllPlayers();

            foreach (var victim in characters)
            {
                if (victim.getPosition().distanceSq(pos) <= 50000)
                {
                    victim.saveLocationOnWarp();
                    await victim.changeMap(target, target.getRandomPlayerSpawnpoint());
                }
            }
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
            await player.Yellow(nameof(ClientMessage.MapNotFound));
        }
    }
}
