using Application.Core.Login.Client;
using Application.Core.Login.Net.Packets;
using Application.Core.Login.Session;
using Application.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.Net.Handlers;

public class CharSelectedHandler : OnCharacterSelectedHandler
{
    public CharSelectedHandler(MasterServer server, ILogger<LoginHandlerBase> logger, SessionCoordinator sessionCoordinator)
        : base(server, logger, sessionCoordinator)
    {
    }

    public override async Task HandlePacket(InPacket p, ILoginClient c)
    {
        int charId = p.readInt();

        /// hwid.fromHostString 中提到hostString分为2部分，第1部分是mac,第2部分是hwid, 那这里获取的macs又是什么
        string macs = p.readString();
        string hostString = p.readString();

        await Process(c, charId, hostString, macs);
    }
}