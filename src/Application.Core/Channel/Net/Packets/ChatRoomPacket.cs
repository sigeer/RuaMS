using tools;

namespace Application.Core.Channel.Net.Packets
{
    public class ChatRoomPacket
    {
        public static Packet addMessengerPlayer(string from, ProtoModel.PlayerViewProto chr, int position)
        {
            OutPacket p = OutPacket.create(SendOpcode.MESSENGER);
            p.writeByte(0x00);
            p.writeByte(position);
            PacketCreator.addCharLook(p, chr, true);
            p.writeString(from);
            p.writeByte(chr.Channel - 1);
            p.writeByte(0x00);
            return p;
        }

        public static Packet joinMessenger(int position)
        {
            OutPacket p = OutPacket.create(SendOpcode.MESSENGER);
            p.writeByte(0x01);
            p.writeByte(position);
            return p;
        }
    }
}
