using client.keybind;

namespace Application.Core.Channel.Net.Handlers;

/**
 * @author Shavit
 */
public class QuickslotKeyMappedModifiedHandler : ChannelHandlerBase
{
    public override void HandlePacket(InPacket p, IChannelClient c)
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
