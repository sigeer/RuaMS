using scripting;
using server.quest;

namespace Application.Core.Game.Commands.Gm6;

public class ClearCacheCommand : ParamsCommandBase
{
    public ClearCacheCommand() : base(["[quest|script]"], 6, "clearcache")
    {
        Description = "Clear all cache.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var p = GetParamByIndex(0);
        if (string.IsNullOrEmpty(p))
            throw new CommandArgumentException();

        if (p.Equals("quest", StringComparison.OrdinalIgnoreCase))
        {
            Quest.clearCache();
            c.OnlinedCharacter.dropMessage(5, "Quest Cache Cleared.");
        }

        if (p.Equals("script", StringComparison.OrdinalIgnoreCase))
        {
            AbstractScriptManager.ClearCache();
            c.OnlinedCharacter.dropMessage(5, "Script Cache Cleared.");
        }

    }
}
