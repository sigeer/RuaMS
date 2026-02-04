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
        var scriptEngine = client.CurrentServer.DevtestScriptManager.GetScriptEngine("devtest");
        try
        {
            scriptEngine?.CallFunction("run", client.Character);
        }
        catch (Exception e)
        {
            log.Information(e, "devtest.js run() threw an exception");
        }
        return;
    }
}

public class DevtestScriptManager : AbstractScriptManager
{
    public DevtestScriptManager(ILogger<AbstractScriptManager> logger, CommandExecutor commandExecutor, WorldChannel worldChannel, IEnumerable<IAddtionalRegistry> addtionalRegistries)
        : base(logger, commandExecutor, worldChannel, addtionalRegistries)
    {
    }

    public IEngine? GetScriptEngine(string path)
    {
        return base.getInvocableScriptEngine(new ScriptFile("", path, ScriptType.Js));
    }

}