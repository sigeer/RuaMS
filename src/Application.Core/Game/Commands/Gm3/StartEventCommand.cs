using server.events.gm;
using tools;

namespace Application.Core.Game.Commands.Gm3;

public class StartEventCommand : CommandBase
{
    public StartEventCommand() : base(3, "startevent")
    {
        Description = "Start an event on current map.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        int players = 50;
        if (paramsValue.Length > 1)
        {
            players = int.Parse(paramsValue[0]);
        }
        c.getChannelServer().setEvent(new Event(player.getMapId(), players));
        c.CurrentServerContainer.SendBroadcastWorldPacket(
            PacketCreator.earnTitleMessage(
                "[Event] An event has started on "
                        + player.getMap().getMapName()
                        + " and will allow "
                        + players
                        + " players to join. Type @joinevent to participate."));
        c.CurrentServerContainer.SendBroadcastWorldPacket(
                PacketCreator.serverNotice(6, "[Event] An event has started on "
                        + player.getMap().getMapName()
                        + " and will allow "
                        + players
                        + " players to join. Type @joinevent to participate."));
    }
}
