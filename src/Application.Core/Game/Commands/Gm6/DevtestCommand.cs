using Microsoft.Extensions.Logging;
using scripting;

namespace Application.Core.Game.Commands.Gm6;

public class DevtestCommand : CommandBase
{
    readonly DevtestScriptManager devtestScriptManager;
    public DevtestCommand(DevtestScriptManager devtestScriptManager) : base(6, "devtest")
    {
        Description = "Runs devtest.js. Developer utility - test stuff without restarting the server.";
        this.devtestScriptManager = devtestScriptManager;
    }



    public override void Execute(IChannelClient client, string[] paramsValue)
    {
        var scriptEngine = devtestScriptManager.GetScriptEngine("devtest.js");
        try
        {
            scriptEngine?.CallFunction("run", client.Character);
        }
        catch (Exception e)
        {
            log.Information(e, "devtest.js run() threw an exception");
        }
    }
}

public class DevtestScriptManager : AbstractScriptManager
{
    public DevtestScriptManager(ILogger<AbstractScriptManager> logger, CommandExecutor commandExecutor) : base(logger, commandExecutor)
    {
    }

    public IEngine? GetScriptEngine(string path)
    {
        return base.getInvocableScriptEngine(path);
    }

}