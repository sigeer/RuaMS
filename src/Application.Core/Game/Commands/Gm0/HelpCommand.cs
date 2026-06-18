namespace Application.Core.Game.Commands.Gm0;

public class HelpCommand : CommandBase
{
    public HelpCommand() : base(0, "help", "commands")
    {
    }

    public override async Task Execute(IChannelClient client, string[] paramsValue)
    {
        await client.OnlinedCharacter.OpenNpc(NpcId.STEWARD, "commands");
    }
}
