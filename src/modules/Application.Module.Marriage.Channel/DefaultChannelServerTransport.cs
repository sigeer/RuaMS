using Application.Core.ServerTransports;
using Application.Module.Marriage.Common;
using MarriageProto;

namespace Application.Module.Marriage.Channel
{
    public class DefaultModuleChannelServerTransport : IModuleChannelServerTransport
    {
        readonly MarriageServiceProto.ChannelService.ChannelServiceClient _grpcClient;
        IChannelServerTransport _transport;
        public DefaultModuleChannelServerTransport(MarriageServiceProto.ChannelService.ChannelServiceClient client, IChannelServerTransport transport)
        {
            _grpcClient = client;
            _transport = transport;
        }

        public async Task BreakMarriage(BreakMarriageRequest breakMarriageRequest)
        {
            await _transport.SendAsync(ChannelSend.BreakMarriage, breakMarriageRequest);
        }

        public void CloseWedding(CloseWeddingRequest closeWeddingRequest)
        {
            _grpcClient.CloseWedding(closeWeddingRequest);
        }

        public CompleteWeddingResponse CompleteWedding(CompleteWeddingRequest completeWeddingRequest)
        {
            return _grpcClient.CompleteWedding(completeWeddingRequest);
        }

        public CreateMarriageRelationResponse CreateMarriageRelation(CreateMarriageRelationRequest createMarriageRelationRequest)
        {
            return _grpcClient.CreateMarriageRelation(createMarriageRelationRequest);
        }

        public WeddingInfoListDto LoadAllWeddingById(LoadWeddingByIdRequest request)
        {
            return _grpcClient.LoadWeddings(request);
        }

        public object LoadMarriageGifts(LoadMarriageGiftsRequest loadMarriageGiftsRequest)
        {
            throw new NotImplementedException();
        }

        public LoadMarriageInfoResponse LoadMarriageInfo(LoadMarriageInfoRequest loadMarriageInfoRequest)
        {
            return _grpcClient.LoadEffectMarriageInfo(loadMarriageInfoRequest);
        }

        public ReserveWeddingResponse ReserveWedding(ReserveWeddingRequest reserveWeddingRequest)
        {
            return _grpcClient.ReserveWedding(reserveWeddingRequest);
        }

        public async Task SendSpouseChat(SendSpouseChatRequest sendSpouseChatRequest)
        {
            await _transport.SendAsync(ChannelSend.SpouseChat, sendSpouseChatRequest);
        }

        public LoadInvitationResponse TryGetInvitationInfo(LoadInvitationRequest loadInvitationRequest)
        {
            return _grpcClient.LoadInvitationContent(loadInvitationRequest);
        }

        public async Task TryInviteGuest(InviteGuestRequest inviteGuestRequest)
        {
            await _transport.SendAsync(ChannelSend.WeddingInviteGuest, inviteGuestRequest);
        }
    }
}
