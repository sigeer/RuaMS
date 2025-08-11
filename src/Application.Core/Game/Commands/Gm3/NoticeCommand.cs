namespace Application.Core.Game.Commands.Gm3;

public class NoticeCommand : CommandBase
{
    public NoticeCommand() : base(3, "notice")
    {
        Description = "Send a blue message to everyone on the server.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        c.CurrentServerContainer.SendDropMessage(6, "[Notice] " + player.getLastCommandMessage());
    }
}
