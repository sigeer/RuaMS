using net.packet;
using server;
using tools;

namespace Application.Core.Channel.Net.Handlers;

/**
 * @author kevintjuh93
 */
public class AdminChatHandler : ChannelHandlerBase
{

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        if (!c.OnlinedCharacter.isGM())
        {//if ( (signed int)CWvsContext::GetAdminLevel((void *)v294) > 2 )
            return;
        }
        byte mode = p.readByte();
        //not saving slides...
        string message = p.readString();
        var packet = PacketCreator.serverNotice(p.readByte(), message);//maybe I should make a check for the slea.readByte()... but I just hope gm's don't fuck things up :)
        switch (mode)
        {
            case 0:
                {// /alertall, /noticeall, /slideall
                    c.CurrentServer.BroadcastWorldMessage(packet);
                    // ChatLogger.log(c, "Alert All", message);
                    break;
                }
            case 1:
                {// /alertch, /noticech, /slidech
                    c.CurrentServer.broadcastPacket(packet);
                    // ChatLogger.log(c, "Alert Ch", message);
                    break;
                }
            case 2:
                {// /alertm /alertmap, /noticem /noticemap, /slidem /slidemap
                    c.OnlinedCharacter.getMap().broadcastMessage(packet);
                    // ChatLogger.log(c, "Alert Map", message);
                    break;
                }
        }
    }
}
