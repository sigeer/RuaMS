using Application.Core.Client;
using Application.Core.Login.Database;
using Application.Core.Login.Session;
using Application.Core.Servers;
using Microsoft.Extensions.Logging;
using net.packet;

namespace Application.Core.Login.Net.Handlers;


/// <summary>
/// 从所有角色中选择角色
/// </summary>
public class ViewAllCharSelectedWithPicHandler : OnCharacterSelectedWithPicHandler
{
    public ViewAllCharSelectedWithPicHandler(IMasterServer server, AccountManager accountManager, ILogger<LoginHandlerBase> logger, SessionCoordinator sessionCoordinator)
        : base(server, accountManager, logger, sessionCoordinator)
    {
    }

    public override void HandlePacket(InPacket p, ILoginClient c)
    {

        string pic = p.readString();
        int charId = p.readInt();
        p.readInt(); // please don't let the client choose which world they should login

        string macs = p.readString();
        string hostString = p.readString();

        Process(c, charId, pic, hostString, macs);
    }
}
