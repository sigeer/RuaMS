using Application.Core.Channel.Services;
using Application.Core.scripting.npc;
using net.server;

namespace Application.Core.Game.Commands.Gm5;

/**
 * @author Mist
 * @author Blood (Tochi)
 * @author Ronan
 */
public class IpListCommand : CommandBase
{
    readonly AdminService _adminService;
    public IpListCommand(AdminService adminService) : base(5, "iplist")
    {
        _adminService = adminService;
        Description = "Show IP of all players.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        string str = "Player-IP relation:";

        var clientInfos = _adminService.GetOnliendClientInfo();

        foreach (var chr in clientInfos)
        {
            str += "  " + chr.CharacterName + " - " + chr.CurrentIP + "\r\n";
        }

        TempConversation.Create(c, 22000)?.RegisterTalk(str);
    }

}
