using Application.Core.Game.Relation;

namespace Application.Core.Channel.Net.Packets
{
    public class NewPacketCreator
    {
        protected virtual OutPacket WriteMarriageRing(OutPacket p, IPlayer chr)
        {
            p.writeShort(0);
            return p;
        }
    }
}
