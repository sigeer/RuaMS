using Application.Core.model;
using Application.Shared.Configs;

namespace Application.Core.ServerTransports
{
    public interface IMasterServerTransport : IServerTransport
    {
        void SendServerMessage(IEnumerable<int> playerIdList);
        void SendServerMessage();
        CoupleIdPair? GetAllWeddingCoupleForGuest(int guestId, bool cathedral);
        int GetAllWeddingReservationStatus(IEnumerable<int> pw, bool cathedral);
        void SendWorldConfig(WorldConfigPatch patch);
    }
}
