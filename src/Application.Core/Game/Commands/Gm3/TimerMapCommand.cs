using Application.Resources.Messages;
using tools;

namespace Application.Core.Game.Commands.Gm3;

public class TimerMapCommand : CommandBase
{
    public TimerMapCommand() : base(3, "timermap")
    {
    }

    public override Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.YellowMessageI18N(nameof(ClientMessage.TimerMapCommand_Syntax));
            return Task.CompletedTask;
        }

        if (paramsValue[0].Equals("remove", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var victim in player.getMap().getAllPlayers())
            {
                victim.sendPacket(PacketCreator.removeClock());
            }
        }
        else
        {
            try
            {
                int seconds = int.Parse(paramsValue[0]);
                foreach (var victim in player.getMap().getAllPlayers())
                {
                    victim.sendPacket(PacketCreator.getClock(seconds));
                }
            }
            catch (FormatException e)
            {
                log.Warning(e.ToString());
                player.YellowMessageI18N(nameof(ClientMessage.TimerMapCommand_Syntax));
            }
        }
        return Task.CompletedTask;
    }
}
