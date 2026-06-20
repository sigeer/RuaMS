using Application.Core.Channel.ServerData;

namespace Application.Core.Game.Commands.Gm3;

public class IgnoredCommand : CommandBase
{
    readonly AutoBanDataManager _autoBanManager;
    public IgnoredCommand(AutoBanDataManager autoBanManager) : base(3, "ignored")
    {
        Description = "Show all characters being ignored in auto-ban alerts.";
        _autoBanManager = autoBanManager;
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        foreach (var item in _autoBanManager.GetAutobanIngores())
        {
            await player.Yellow(item.Value + " is being ignored.");
        }
    }
}
