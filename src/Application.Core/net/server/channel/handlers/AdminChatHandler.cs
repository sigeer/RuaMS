

using client;
using net.packet;
using server;
using tools;

namespace net.server.channel.handlers;

/**
 * @author kevintjuh93
 */
public class AdminChatHandler : AbstractPacketHandler
{

    public override void handlePacket(InPacket p, Client c)
    {
        if (!c.getPlayer().isGM())
        {//if ( (signed int)CWvsContext::GetAdminLevel((void *)v294) > 2 )
            return;
        }
        byte mode = p.readByte();
        //not saving slides...
        string message = p.readString();
        Packet packet = PacketCreator.serverNotice(p.readByte(), message);//maybe I should make a check for the slea.readByte()... but I just hope gm's don't fuck things up :)
        switch (mode)
        {
            case 0:
                {// /alertall, /noticeall, /slideall
                    c.getWorldServer().broadcastPacket(packet);
                    ChatLogger.log(c, "Alert All", message);
                    break;
                }
            case 1:
                {// /alertch, /noticech, /slidech
                    c.getChannelServer().broadcastPacket(packet);
                    ChatLogger.log(c, "Alert Ch", message);
                    break;
                }
            case 2:
                {// /alertm /alertmap, /noticem /noticemap, /slidem /slidemap
                    c.getPlayer().getMap().broadcastMessage(packet);
                    ChatLogger.log(c, "Alert Map", message);
                    break;
                }
        }
    }
}
