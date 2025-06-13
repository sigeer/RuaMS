

using Application.Core.Channel.ServerData;

namespace Application.Core.Channel.Net.Handlers;

public class FamilySummonResponseHandler : ChannelHandlerBase
{
    readonly FamilyManager _familyManager;

    public FamilySummonResponseHandler(FamilyManager familyManager)
    {
        _familyManager = familyManager;
    }

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        if (!YamlConfig.config.server.USE_FAMILY_SYSTEM)
        {
            return;
        }
        p.readString(); //family name
        bool accept = p.readByte() != 0;
        _familyManager.AnswerSummonInvite(c.OnlinedCharacter, accept);
    }

}
