using Application.Resources.Messages;
using tools;

namespace Application.Core.Game.Commands.Gm3;

public class TimerCommand : CommandBase
{
    public TimerCommand() : base(3, "timer")
    {
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 2)
        {
            await player.Yellow(nameof(ClientMessage.TimerCommand_Syntax));
            return;
        }

        var victim = player.getMap().getCharacterByName(paramsValue[0]);
        if (victim != null && victim.IsOnlined)
        {
            if (paramsValue[1].Equals("remove", StringComparison.OrdinalIgnoreCase))
            {
                await victim.SendPacket(PacketCreator.removeClock());
            }
            else
            {
                try
                {
                    await victim.SendPacket(PacketCreator.getClock(int.Parse(paramsValue[1])));
                }
                catch (FormatException e)
                {
                    log.Error(e.ToString());
                    await player.Yellow(nameof(ClientMessage.TimerCommand_Syntax));
                }
            }
        }
        else
        {
            await player.Yellow(nameof(ClientMessage.PlayerNotFoundInMap), paramsValue[0]);
        }
    }
}
