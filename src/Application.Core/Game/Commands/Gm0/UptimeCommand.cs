using net.server;

namespace Application.Core.Game.Commands.Gm0;

public class UptimeCommand : CommandBase
{
    public UptimeCommand() : base(0, "uptime")
    {
        Description = "Show server online time.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var dur = DateTimeOffset.UtcNow - Server.uptime;
        c.OnlinedCharacter.yellowMessage("NewServer has been online for " + dur.Days + " days " + dur.Hours + " hours " + dur.Minutes + " minutes and " + dur.Seconds + " seconds.");
    }
}
