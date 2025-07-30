using Application.Core.Channel;
using Grpc.Net.Client;
using MarriageProto;

namespace Application.Module.Marriage.Channel
{
    public class DefaultChannelServerTransport : IChannelServerTransport
    {
        readonly MarriageServiceProto.ChannelService.ChannelServiceClient _grpcClient;
        public DefaultChannelServerTransport(WorldChannelServer server)
        {
            _grpcClient = new MarriageServiceProto.ChannelService.ChannelServiceClient(GrpcChannel.ForAddress(server.ServerConfig.MasterServerGrpcAddress));
        }

        public void BreakMarriage(BreakMarriageRequest breakMarriageRequest)
        {
            _grpcClient.BreakMarriage(breakMarriageRequest);
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

        public SendSpouseChatResponse SendSpouseChat(SendSpouseChatRequest sendSpouseChatRequest)
        {
            return _grpcClient.SpouseChat(sendSpouseChatRequest);
        }

        public LoadInvitationResponse TryGetInvitationInfo(LoadInvitationRequest loadInvitationRequest)
        {
            return _grpcClient.LoadInvitationContent(loadInvitationRequest);
        }

        public InviteGuestResponse TryInviteGuest(InviteGuestRequest inviteGuestRequest)
        {
            return _grpcClient.InviteGuest(inviteGuestRequest);
        }
    }
}
