using tools;

namespace Application.Core.Game.Commands.Gm2;

public class UnBugCommand : CommandBase
{
    public UnBugCommand() : base(2, "unbug")
    {
        Description = "Unbug self.";
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        await c.OnlinedCharacter.getMap().broadcastMessage(PacketCreator.enableActions());
    }
}
