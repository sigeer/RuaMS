namespace Application.Core.Game.Commands.Gm4;

public class ServerMessageCommand : CommandBase
{
    public ServerMessageCommand() : base(4, "servermessage")
    {
        Description = "Set scrolling server message.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        c.getChannelServer().Container.Transport.SendWorldConfig(new Config.WorldConfig { ServerMessage = player.getLastCommandMessage() });
    }
}
