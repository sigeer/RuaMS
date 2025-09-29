using Application.Resources.Messages;
using tools;

namespace Application.Core.Game.Commands.Gm3;

public class KillCommand : CommandBase
{
    public KillCommand() : base(3, "kill")
    {
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.YellowMessageI18N(nameof(ClientMessage.KillCommand_Syntax));
            return;
        }

        var victim = c.CurrentServer.getPlayerStorage().getCharacterByName(paramsValue[0]);
        if (victim != null && victim.IsOnlined)
        {
            victim.KilledBy(player);
            c.CurrentServerContainer.SendBroadcastWorldGMPacket(PacketCreator.serverNotice(5, player.getName() + " used !kill on " + victim.getName()));
        }
        else
        {
            player.YellowMessageI18N(nameof(ClientMessage.PlayerNotFoundInChannel), paramsValue[0]);
        }
    }
}
