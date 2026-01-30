namespace Application.Core.Channel.Services
{
    public interface IMarriageService
    {
        void WriteMarriageRing(OutPacket p, Player chr);
    }

    public class DefaultMarriageService : IMarriageService
    {
        public void WriteMarriageRing(OutPacket p, Player chr)
        {
            p.writeShort(0);
        }
    }
}
