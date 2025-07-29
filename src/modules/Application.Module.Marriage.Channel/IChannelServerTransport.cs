using ItemProto;
using MarriageProto;

namespace Application.Module.Marriage.Channel
{
    public interface IChannelServerTransport
    {
        void BreakMarriage(BreakMarriageRequest breakMarriageRequest);
        void CloseWedding(CloseWeddingRequest closeWeddingRequest);
        CompleteWeddingResponse CompleteWedding(CompleteWeddingRequest completeWeddingRequest);
        CreateMarriageRelationResponse CreateMarriageRelation(CreateMarriageRelationRequest createMarriageRelationRequest);
        WeddingInfoListDto LoadAllWeddingById(LoadWeddingByIdRequest request);
        object LoadMarriageGifts(LoadMarriageGiftsRequest loadMarriageGiftsRequest);
        MarriageProto.LoadMarriageInfoResponse LoadMarriageInfo(LoadMarriageInfoRequest loadMarriageInfoRequest);
        MarriageProto.ReserveWeddingResponse ReserveWedding(ReserveWeddingRequest reserveWeddingRequest);
        SendSpouseChatResponse SendSpouseChat(SendSpouseChatRequest sendSpouseChatRequest);
        LoadInvitationResponse TryGetInvitationInfo(LoadInvitationRequest loadInvitationRequest);
        InviteGuestResponse TryInviteGuest(InviteGuestRequest inviteGuestRequest);
    }
}
