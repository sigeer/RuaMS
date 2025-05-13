using Application.Core.Client;
using Application.Core.Login.Database;
using Application.Core.Servers;
using Microsoft.Extensions.Logging;
using net.packet;

namespace Application.Core.Login.Net.Handlers;



public class CharSelectedWithPicHandler : OnCharacterSelectedWithPicHandler
{
    public CharSelectedWithPicHandler(IMasterServer server, AccountManager accountManager, ILogger<LoginHandlerBase> logger)
        : base(server, accountManager, logger)
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
