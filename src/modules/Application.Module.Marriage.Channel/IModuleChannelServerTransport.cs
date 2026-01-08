using ItemProto;
using MarriageProto;

namespace Application.Module.Marriage.Channel
{
    public interface IModuleChannelServerTransport
    {
        Task BreakMarriage(BreakMarriageRequest breakMarriageRequest);
        void CloseWedding(CloseWeddingRequest closeWeddingRequest);
        CompleteWeddingResponse CompleteWedding(CompleteWeddingRequest completeWeddingRequest);
        CreateMarriageRelationResponse CreateMarriageRelation(CreateMarriageRelationRequest createMarriageRelationRequest);
        WeddingInfoListDto LoadAllWeddingById(LoadWeddingByIdRequest request);
        object LoadMarriageGifts(LoadMarriageGiftsRequest loadMarriageGiftsRequest);
        MarriageProto.LoadMarriageInfoResponse LoadMarriageInfo(LoadMarriageInfoRequest loadMarriageInfoRequest);
        MarriageProto.ReserveWeddingResponse ReserveWedding(ReserveWeddingRequest reserveWeddingRequest);
        Task SendSpouseChat(SendSpouseChatRequest sendSpouseChatRequest);
        LoadInvitationResponse TryGetInvitationInfo(LoadInvitationRequest loadInvitationRequest);
        Task TryInviteGuest(InviteGuestRequest inviteGuestRequest);
    }
}
