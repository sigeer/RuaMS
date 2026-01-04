using Application.Core.Channel.Net;
using Application.Core.Client;
using Application.Module.Family.Channel.Net.Packets;
using Application.Shared.Net;
using Microsoft.Extensions.Logging;

namespace Application.Module.Family.Channel.Net.Handlers;

public class AcceptFamilyHandler : ChannelHandlerBase
{
    protected ILogger<AcceptFamilyHandler> _logger;
    readonly FamilyManager _familyManager;

    public AcceptFamilyHandler(ILogger<AcceptFamilyHandler> logger, FamilyManager familyManager)
    {
        _logger = logger;
        _familyManager = familyManager;
    }

    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        var chr = c.OnlinedCharacter;
        int inviterId = p.readInt();
        var str = p.readString();
        bool accept = p.readByte() != 0;
        // string inviterName = slea.readMapleAsciiString();
        await _familyManager.AnswerInvite(c.OnlinedCharacter, -1, accept);
        c.sendPacket(FamilyPacketCreator.sendFamilyMessage(0, 0));
    }
}
