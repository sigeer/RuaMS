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

        // GetLocalMacAddress：所有网卡的mac
        string macs = p.readString();
        // GetLocalMacAddressWithHDDSerialNo 第一个有效网卡的mac + hwid
        string hostString = p.readString();

        await Process(c, charId, hostString, macs);
    }
}