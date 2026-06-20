namespace Application.Core.Game.Commands.Gm3;

public class ClosePortalCommand : CommandBase
{
    public ClosePortalCommand() : base(3, "closeportal")
    {
        Description = "Close a portal.";
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            await player.Yellow("Syntax: !closeportal <portalid>");
            return;
        }
        var portal = player.getMap().getPortal(paramsValue[0]);
        if (portal == null)
        {
            await player.Yellow("invalid portalid");
            return;
        }
        portal.setPortalState(false);
    }
}
