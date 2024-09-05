using client.keybind;
using net.packet;

namespace net.server.channel.handlers;

/**
 * @author Shavit
 */
public class QuickslotKeyMappedModifiedHandler : AbstractPacketHandler
{
    public override void HandlePacket(InPacket p, IClient c)
    {
        // Invalid size for the packet.
        if (p.available() != QuickslotBinding.QUICKSLOT_SIZE * sizeof(int) ||
                // not logged in-game
                c.OnlinedCharacter == null)
        {
            return;
        }

        byte[] aQuickslotKeyMapped = new byte[QuickslotBinding.QUICKSLOT_SIZE];

        for (int i = 0; i < QuickslotBinding.QUICKSLOT_SIZE; i++)
        {
            aQuickslotKeyMapped[i] = (byte)p.readInt();
        }

        c.OnlinedCharacter.changeQuickslotKeybinding(aQuickslotKeyMapped);
    }
}
