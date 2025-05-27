namespace Application.Core.Game.Commands.Gm0;

public class HelpCommand : CommandBase
{
    public HelpCommand() : base(0, "help", "commands")
    {
        Description = "Show available commands.";
    }

    public override void Execute(ChannelClient client, string[] paramsValue)
    {
        client.OpenNpc(NpcId.STEWARD, "commands");
    }
}
