using Application.Core.Login;
using Application.Core.Login.ServerTransports;
using Application.Module.Marriage.Common;
using MarriageProto;

namespace Application.Module.Marriage.Master
{
    public class MasterTransport: MasterServerTransportBase
    {
        public MasterTransport(MasterServer masterServer) : base(masterServer)
        {
        }

        internal void BroadcastWedding(BroadcastWeddingDto broadcastWeddingDto)
        {
            BroadcastMessage(MessageType.WeddingBroadcast, broadcastWeddingDto);
        }

        internal void ReturnGuestInvitation(InviteGuestResponse inviteGuestResponse)
        {
            throw new NotImplementedException();
        }
    }
}
