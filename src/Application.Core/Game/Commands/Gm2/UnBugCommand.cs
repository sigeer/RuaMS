using tools;

namespace Application.Core.Game.Commands.Gm2;

public class UnBugCommand : CommandBase
{
    public UnBugCommand() : base(2, "unbug")
    {
        Description = "Unbug self.";
    }

    public override Task Execute(IChannelClient c, string[] paramsValue)
    {
        c.OnlinedCharacter.getMap().broadcastMessage(PacketCreator.enableActions());
        return Task.CompletedTask;
    }
}
