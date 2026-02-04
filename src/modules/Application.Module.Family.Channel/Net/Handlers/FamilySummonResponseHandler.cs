using Application.Core.Channel.Net;
using Application.Core.Client;
using Application.Shared.Net;

namespace Application.Module.Family.Channel.Net.Handlers;

public class FamilySummonResponseHandler : ChannelHandlerBase
{
    readonly FamilyManager _familyManager;

    public FamilySummonResponseHandler(FamilyManager familyManager)
    {
        _familyManager = familyManager;
    }

    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        p.readString(); //family name
        bool accept = p.readByte() != 0;
        await _familyManager.AnswerSummonInvite(c.OnlinedCharacter, -1, accept);
    }

}
