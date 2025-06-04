using Application.Core.Channel;
using Microsoft.Extensions.Logging;
using scripting;

namespace Application.Core.Game.Commands.Gm6;

public class DevtestCommand : CommandBase
{
    public DevtestCommand() : base(6, "devtest")
    {
        Description = "Runs devtest.js. Developer utility - test stuff without restarting the server.";
    }

    public override void Execute(IChannelClient client, string[] paramsValue)
    {
        var scriptEngine = client.CurrentServer.DevtestScriptManager.GetScriptEngine("devtest.js");
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
    public DevtestScriptManager(ILogger<AbstractScriptManager> logger, CommandExecutor commandExecutor, WorldChannel worldChannel) : base(logger, commandExecutor, worldChannel)
    {
    }

    public IEngine? GetScriptEngine(string path)
    {
        return base.getInvocableScriptEngine(new ScriptFile("", path));
    }

}