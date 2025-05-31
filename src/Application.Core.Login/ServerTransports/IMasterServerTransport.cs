using Application.Shared.Configs;
using Application.Shared.Servers;

namespace Application.Core.ServerTransports
{
    public interface IMasterServerTransport : IServerTransport
    {
        void SendServerMessage(IEnumerable<int> playerIdList);
        CoupleIdPair? GetAllWeddingCoupleForGuest(int guestId, bool cathedral);
        int GetAllWeddingReservationStatus(IEnumerable<int> pw, bool cathedral);
        void SendWorldConfig(WorldConfigPatch patch);
        bool WrapPlayer(string name, int? channel, int mapId, int? portal);
        void BroadcastWorldGMPacket(Packet packet);
        void BroadcastMessage(Packet p);
        void SendNotes(int channel, int id, Dto.NoteDto[] notes);
        void SendUpdateCouponRates(Config.CouponConfig config);
        void SendDueyNotification(int channel, int id, string senderName, bool dueyType);
    }
}
