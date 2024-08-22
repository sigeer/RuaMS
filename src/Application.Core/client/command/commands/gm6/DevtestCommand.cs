using Microsoft.ClearScript.V8;
using scripting;

namespace client.command.commands.gm6;

public class DevtestCommand : Command
{
    public DevtestCommand()
    {
        setDescription("Runs devtest.js. Developer utility - test stuff without restarting the server.");
    }

    private class DevtestScriptManager : AbstractScriptManager
    {

        public V8ScriptEngine? GetScriptEngine(string path)
        {
            return base.getInvocableScriptEngine(path);
        }

    }

    public override void execute(Client client, string[] paramsValue)
    {
        DevtestScriptManager scriptManager = new DevtestScriptManager();
        var scriptEngine = scriptManager.GetScriptEngine("devtest.js");
        try
        {
            scriptEngine?.InvokeSync("run", client.getPlayer());
        }
        catch (Exception e)
        {
            log.Information(e, "devtest.js run() threw an exception");
        }
    }
}
