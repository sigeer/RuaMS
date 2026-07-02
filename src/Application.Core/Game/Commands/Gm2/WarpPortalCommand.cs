using server.maps;
using tools;

namespace Application.Core.Game.Commands.Gm2;

public class WarpPortalCommand : ParamsCommandBase
{
    public WarpPortalCommand() : base(["<portalInput>"], 2, "warpportal")
    {
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        var mapFactory = c.CurrentServer.getMapFactory();

        var input = GetParam("portalInput");
        Portal? portal = null;
        if (int.TryParse(input, out var portalId))
        {
            portal = player.MapModel.getPortal(portalId);
        }
        else
        {
            portal = player.MapModel.getPortal(input);
        }

        if (portal != null)
        {
            await player.SendPacket(PacketCreator.TeleportPortal(false, portalId));
            return;
        }
        else
        {
            await player.Yellow("Portal Id not found");
            return;
        }
    }
}
