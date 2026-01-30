namespace Application.Core.Channel.Net.Packets
{
    public class NewPacketCreator
    {
        protected virtual OutPacket WriteMarriageRing(OutPacket p, Player chr)
        {
            p.writeShort(0);
            return p;
        }
    }
}
