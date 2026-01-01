namespace Application.Core.Game.Commands.Gm3;

public class NoticeCommand : CommandBase
{
    public NoticeCommand() : base(3, "notice")
    {
        Description = "Send a blue message to everyone on the server.";
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        await c.CurrentServerContainer.SendDropMessage(6, "[Notice] " + player.getLastCommandMessage());
    }
}
