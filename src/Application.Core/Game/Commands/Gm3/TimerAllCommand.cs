using tools;

namespace Application.Core.Game.Commands.Gm3;

public class TimerAllCommand : CommandBase
{
    public TimerAllCommand() : base(3, "timerall")
    {
        Description = "Set a server wide timer.";
    }

    public override void Execute(IClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !timerall <seconds>|remove");
            return;
        }

        if (paramsValue[0].Equals("remove", StringComparison.OrdinalIgnoreCase))
        {
            player.getChannelServer().Transport.RemoveTimer();
        }
        else
        {
            try
            {
                int seconds = int.Parse(paramsValue[0]);
                player.getChannelServer().Transport.SendTimer(seconds);
            }
            catch (FormatException e)
            {
                log.Error(e.ToString());
                player.yellowMessage("Syntax: !timerall <seconds>|remove");
            }
        }
    }
}
