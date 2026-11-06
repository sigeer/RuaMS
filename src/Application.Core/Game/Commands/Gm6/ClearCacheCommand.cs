using Application.Core.Channel.DataProviders;
using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm6;

public class ClearCacheCommand : ParamsCommandBase
{
    public ClearCacheCommand() : base(["[quest]"], 6, "clearcache")
    {
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var p = GetParamByIndex(0);
        if (string.IsNullOrEmpty(p))
            throw new CommandArgumentException();

        if (p.Equals("quest", StringComparison.OrdinalIgnoreCase))
        {
            QuestFactory.Instance.clearCache();
            await c.OnlinedCharacter.Yellow(nameof(ClientMessage.Command_Done), c.OnlinedCharacter.getLastCommandMessage());
        }
    }
}
