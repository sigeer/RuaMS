using Application.Core.Channel.ServerData;

namespace Application.Core.Game.Commands.Gm3;

public class IgnoreCommand : CommandBase
{
    readonly AutoBanDataManager _autoBanManager;
    public IgnoreCommand(AutoBanDataManager autoBanManager) : base(3, "ignore")
    {
        Description = "Toggle ignore a character from auto-ban alerts.";
        _autoBanManager = autoBanManager;
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            await player.Yellow("Syntax: !ignore <ign>");
            return;
        }

        _autoBanManager.ToggleIgnore(c.OnlinedCharacter, paramsValue[0]);
    }
}
