

using client;
using client.keybind;
using net.packet;

namespace net.server.channel.handlers;

/**
 * @author Shavit
 */
public class QuickslotKeyMappedModifiedHandler : AbstractPacketHandler
{
    public override void handlePacket(InPacket p, Client c)
    {
        // Invalid size for the packet.
        if (p.available() != QuickslotBinding.QUICKSLOT_SIZE * sizeof(int) ||
                // not logged in-game
                c.getPlayer() == null)
        {
            return;
        }

        byte[] aQuickslotKeyMapped = new byte[QuickslotBinding.QUICKSLOT_SIZE];

        for (int i = 0; i < QuickslotBinding.QUICKSLOT_SIZE; i++)
        {
            aQuickslotKeyMapped[i] = (byte)p.readInt();
        }

        c.getPlayer().changeQuickslotKeybinding(aQuickslotKeyMapped);
    }
}
