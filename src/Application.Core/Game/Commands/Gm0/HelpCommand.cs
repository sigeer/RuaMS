namespace Application.Core.Game.Commands.Gm0;

public class HelpCommand : CommandBase
{
    public HelpCommand() : base(0, "help", "commands")
    {
    }

    public override Task Execute(IChannelClient client, string[] paramsValue)
    {
        client.OpenNpc(NpcId.STEWARD, "commands");
        return Task.CompletedTask;
    }
}
