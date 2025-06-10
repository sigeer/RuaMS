using tools;

namespace Application.Core.Game.Commands.Gm3;

public class KillCommand : CommandBase
{
    public KillCommand() : base(3, "kill")
    {
        Description = "Kill a player.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !kill <playername>");
            return;
        }

        var victim = c.CurrentServer.getPlayerStorage().getCharacterByName(paramsValue[0]);
        if (victim != null && victim.IsOnlined)
        {
            victim.KilledBy(player);
            c.CurrentServerContainer.BroadcastWorldGMPacket(PacketCreator.serverNotice(5, player.getName() + " used !kill on " + victim.getName()));
        }
        else
        {
            player.message("Player '" + paramsValue[0] + "' could not be found.");
        }
    }
}
