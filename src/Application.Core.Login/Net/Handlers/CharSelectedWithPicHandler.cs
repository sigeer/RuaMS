using Application.Core.Login.Client;
using Application.Core.Login.Session;
using Microsoft.Extensions.Logging;
using net.packet;

namespace Application.Core.Login.Net.Handlers;



public class CharSelectedWithPicHandler : OnCharacterSelectedWithPicHandler
{
    public CharSelectedWithPicHandler(MasterServer server, ILogger<LoginHandlerBase> logger, SessionCoordinator sessionCoordinator)
        : base(server, logger, sessionCoordinator)
    {
    }

    public override void HandlePacket(InPacket p, ILoginClient c)
    {
        string pic = p.readString();
        int charId = p.readInt();

        string macs = p.readString();
        string hostString = p.readString();

        Process(c, charId, pic, hostString, macs);
    }
}
