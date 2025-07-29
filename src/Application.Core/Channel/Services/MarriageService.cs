namespace Application.Core.Channel.Services
{
    public interface IMarriageService
    {
        void WriteMarriageRing(OutPacket p, IPlayer chr);
    }

    public class DefaultMarriageService : IMarriageService
    {
        public void WriteMarriageRing(OutPacket p, IPlayer chr)
        {
            p.writeShort(0);
        }
    }
}
