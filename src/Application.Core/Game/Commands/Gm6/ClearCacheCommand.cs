using Application.Core.Channel.DataProviders;
using Application.Resources.Messages;
using scripting;

namespace Application.Core.Game.Commands.Gm6;

public class ClearCacheCommand : ParamsCommandBase
{
    public ClearCacheCommand() : base(["[quest|script]"], 6, "clearcache")
    {
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var p = GetParamByIndex(0);
        if (string.IsNullOrEmpty(p))
            throw new CommandArgumentException();

        if (p.Equals("quest", StringComparison.OrdinalIgnoreCase))
        {
            QuestFactory.Instance.clearCache();
            c.OnlinedCharacter.YellowMessageI18N(nameof(ClientMessage.Command_Done), c.OnlinedCharacter.getLastCommandMessage());
        }

        if (p.Equals("script", StringComparison.OrdinalIgnoreCase))
        {
            AbstractScriptManager.ClearCache();
            c.OnlinedCharacter.YellowMessageI18N(nameof(ClientMessage.Command_Done), c.OnlinedCharacter.getLastCommandMessage());
        }

    }
}
