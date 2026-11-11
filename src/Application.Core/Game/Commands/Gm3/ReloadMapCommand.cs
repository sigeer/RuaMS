using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm3;

public class ReloadMapCommand : CommandBase
{
    public ReloadMapCommand() : base(3, "reloadmap")
    {
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;

        await player.MapModel.Reload();
    }
}
