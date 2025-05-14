using net.server;

namespace Application.Core.Game.Commands.Gm2;

public class SummonCommand : CommandBase
{
    public SummonCommand() : base(2, "warphere", "summon")
    {
        Description = "Move a player to your location.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.yellowMessage($"Syntax: !{CurrentCommand} <playername>");
            return;
        }

        var map = player.getMap();

        if (!c.CurrentServer.WarpPlayer(paramsValue[0], player.Channel, player.getMapId(), map.findClosestPortal(player.getPosition())?.getId()))
        {
            player.dropMessage(6, "Unknown player.");
        }
    }
}
