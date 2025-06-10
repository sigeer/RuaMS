using tools;

namespace Application.Core.Game.Commands.Gm3;

public class RipCommand : CommandBase
{
    public RipCommand() : base(3, "rip")
    {
        Description = "Send a RIP notice.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        c.CurrentServerContainer.BroadcastWorldMessage(PacketCreator.serverNotice(6, "[RIP]: " + joinStringFrom(paramsValue, 1)));
    }
}
