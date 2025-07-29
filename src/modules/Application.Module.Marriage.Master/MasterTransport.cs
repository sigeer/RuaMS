using Application.Core.Login;
using Application.Core.Login.ServerTransports;
using Application.Module.Marriage.Common;
using MarriageProto;
using Org.BouncyCastle.Asn1.Ocsp;

namespace Application.Module.Marriage.Master
{
    public class MasterTransport: MasterServerTransportBase
    {
        public MasterTransport(MasterServer masterServer) : base(masterServer)
        {
        }

        internal void SendBreakMarriageCallback(BreakMarriageCallback breakMarriageCallback)
        {
            SendMessage(MessageType.MarriageBroken, breakMarriageCallback, breakMarriageCallback.MasterId, breakMarriageCallback.MasterPartnerId);
        }

        internal void BroadcastWedding(BroadcastWeddingDto broadcastWeddingDto)
        {
            BroadcastMessage(MessageType.WeddingBroadcast, broadcastWeddingDto);
        }

        internal void ReturnGuestInvitation(InviteGuestCallback inviteGuestResponse)
        {
            SendMessage(MessageType.WeddingInviteGuest, inviteGuestResponse, inviteGuestResponse.GuestId);
        }

        internal void SendPlayerTransfter(PlayerTransferDto request)
        {
            SendMessage(MessageType.NotifyPartnerWhenTransfer, request, request.ToPlayerId);
        }

        internal void SendSpouseChat(OnSpouseChatCallback request)
        {
            SendMessage(MessageType.SpouseChat, request, request.SenderId, request.SenderPartnerId);
        }
    }
}
