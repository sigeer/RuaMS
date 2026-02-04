using Application.Core.Login;
using Application.Core.Login.ServerTransports;
using Application.Module.Marriage.Common;
using MarriageProto;
using System.Threading.Tasks;

namespace Application.Module.Marriage.Master
{
    public class MasterTransport : MasterServerTransportBase
    {
        public MasterTransport(MasterServer masterServer) : base(masterServer)
        {
        }

        internal async Task SendBreakMarriageCallback(BreakMarriageResponse breakMarriageCallback)
        {
            await SendMessageN(MasterSend.MarriageBroken, breakMarriageCallback, [breakMarriageCallback.Request.MasterId, breakMarriageCallback.MasterPartnerId]);
        }

        internal async Task BroadcastWedding(BroadcastWeddingDto broadcastWeddingDto)
        {
            await BroadcastMessageN(MasterSend.WeddingBroadcast, broadcastWeddingDto);
        }

        internal async Task SendPlayerTransfter(PlayerTransferDto request)
        {
            await SendMessageN(MasterSend.NotifyPartnerWhenTransfer, request, [request.ToPlayerId]);
        }

        internal async Task SendSpouseChat(SendSpouseChatResponse request)
        {
            await SendMessageN(MasterSend.SpouseChat, request, [request.Request.SenderId, request.SenderPartnerId]);
        }
    }
}
