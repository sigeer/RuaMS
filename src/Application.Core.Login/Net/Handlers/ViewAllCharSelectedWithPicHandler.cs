using Application.Core.Login.Client;
using Application.Core.Login.Session;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.Net.Handlers;


/// <summary>
/// 从所有角色中选择角色
/// </summary>
public class ViewAllCharSelectedWithPicHandler : OnCharacterSelectedWithPicHandler
{
    public ViewAllCharSelectedWithPicHandler(MasterServer server, ILogger<LoginHandlerBase> logger, SessionCoordinator sessionCoordinator)
        : base(server, logger, sessionCoordinator)
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
