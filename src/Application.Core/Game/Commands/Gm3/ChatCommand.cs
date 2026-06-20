namespace Application.Core.Game.Commands.Gm3;

public class ChatCommand : CommandBase
{
    public ChatCommand() : base(3, "togglewhitechat")
    {
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        player.toggleWhiteChat();
        await player.Pink("Your chat is now " + (player.getWhiteChat() ? " white" : "normal") + ".");
    }
}
