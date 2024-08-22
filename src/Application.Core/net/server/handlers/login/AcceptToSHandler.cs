

using client;
using net.packet;
using tools;

namespace net.server.handlers.login;

/**
 * @author kevintjuh93
 */
public class AcceptToSHandler : AbstractPacketHandler
{

    public override bool validateState(Client c)
    {
        return !c.isLoggedIn();
    }

    public override void handlePacket(InPacket p, Client c)
    {
        if (p.available() == 0 || p.readByte() != 1 || c.acceptToS())
        {
            c.disconnect(false, false);//Client dc's but just because I am cool I do this (:
            return;
        }
        if (c.finishLogin() == 0)
        {
            c.sendPacket(PacketCreator.getAuthSuccess(c));
        }
        else
        {
            c.sendPacket(PacketCreator.getLoginFailed(9));//shouldn't happen XD
        }
    }
}
