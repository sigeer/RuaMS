using Application.Resources.Messages;
using tools;

namespace Application.Core.Game.Commands.Gm3;

public class TimerCommand : CommandBase
{
    public TimerCommand() : base(3, "timer")
    {
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 2)
        {
            player.YellowMessageI18N(nameof(ClientMessage.TimerCommand_Syntax));
            return;
        }

        var victim = player.getMap().getCharacterByName(paramsValue[0]);
        if (victim != null && victim.IsOnlined)
        {
            if (paramsValue[1].Equals("remove", StringComparison.OrdinalIgnoreCase))
            {
                victim.sendPacket(PacketCreator.removeClock());
            }
            else
            {
                try
                {
                    victim.sendPacket(PacketCreator.getClock(int.Parse(paramsValue[1])));
                }
                catch (FormatException e)
                {
                    log.Error(e.ToString());
                    player.YellowMessageI18N(nameof(ClientMessage.TimerCommand_Syntax));
                }
            }
        }
        else
        {
            player.YellowMessageI18N(nameof(ClientMessage.PlayerNotFoundInMap), paramsValue[0]);
        }
    }
}
