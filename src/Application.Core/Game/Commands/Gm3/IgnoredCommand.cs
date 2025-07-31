using Application.Core.Channel.ServerData;
using Application.Core.Managers;
using client.autoban;

namespace Application.Core.Game.Commands.Gm3;

public class IgnoredCommand : CommandBase
{
    readonly AutoBanDataManager _autoBanManager;
    public IgnoredCommand(AutoBanDataManager autoBanManager) : base(3, "ignored")
    {
        Description = "Show all characters being ignored in auto-ban alerts.";
        _autoBanManager = autoBanManager;
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        foreach (var item in _autoBanManager.GetAutobanIngores())
        {
            player.yellowMessage(item.Value + " is being ignored.");
        }
    }
}
