

using net.opcodes;

namespace net.packet.outs;

public class SendNoteSuccessPacket : ByteBufOutPacket
{

    public SendNoteSuccessPacket() : base(SendOpcode.MEMO_RESULT)
    {

        writeByte(4);
    }
}
