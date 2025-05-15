using Application.Core.Client;
using Application.Core.Login.Database;
using Application.Core.Login.Session;
using Application.Core.Servers;
using Microsoft.Extensions.Logging;
using net.packet;

namespace Application.Core.Login.Net.Handlers;

public class CharSelectedHandler : OnCharacterSelectedHandler
{
    public CharSelectedHandler(IMasterServer server, AccountManager accountManager, ILogger<LoginHandlerBase> logger, SessionCoordinator sessionCoordinator)
        : base(server, accountManager, logger, sessionCoordinator)
    {
    }

    public override void HandlePacket(InPacket p, ILoginClient c)
    {
        int charId = p.readInt();

        string macs = p.readString();
        string hostString = p.readString();

        Process(c, charId, hostString, macs);
    }
}