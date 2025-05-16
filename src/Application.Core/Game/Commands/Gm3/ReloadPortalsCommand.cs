using scripting.portal;

namespace Application.Core.Game.Commands.Gm3;
public class ReloadPortalsCommand : CommandBase
{
    public ReloadPortalsCommand() : base(3, "reloadportals")
    {
        Description = "Reload all portal scripts.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        c.CurrentServer.PortalScriptManager.reloadPortalScripts();
        player.dropMessage(5, "Reloaded Portals");
    }
}
