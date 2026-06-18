using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm3;

public class TimerAllCommand : CommandBase
{
    public TimerAllCommand() : base(3, "timerall")
    {
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            await player.Yellow(nameof(ClientMessage.TimerAllCommand_Syntax));
            return;
        }

        if (paramsValue[0].Equals("remove", StringComparison.OrdinalIgnoreCase))
        {
            _ = player.getChannelServer().Node.Transport.RemoveTimer();
        }
        else
        {
            try
            {
                int seconds = int.Parse(paramsValue[0]);
                _ = player.getChannelServer().Node.Transport.SendTimer(seconds);
            }
            catch (FormatException e)
            {
                log.Error(e.ToString());
                await player.Yellow(nameof(ClientMessage.TimerAllCommand_Syntax));
            }
        }
    }
}
