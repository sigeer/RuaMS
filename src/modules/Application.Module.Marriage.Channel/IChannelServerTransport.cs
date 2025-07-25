using ItemProto;
using MarriageProto;

namespace Application.Module.Marriage.Channel
{
    public interface IChannelServerTransport
    {
        void BreakMarriage(BreakMarriageRequest breakMarriageRequest);
        RingDto CompleteWedding(CompleteWeddingRequest completeWeddingRequest);
        CreateMarriageRelationResponse CreateMarriageRelation(CreateMarriageRelationRequest createMarriageRelationRequest);
        WeddingInfoListDto LoadAllWeddingById(LoadWeddingByIdRequest request);
        object LoadMarriageGifts(LoadMarriageGiftsRequest loadMarriageGiftsRequest);
        void ReserveWedding(ReserveWeddingRequest reserveWeddingRequest);
        ReserveWeddingCheckResponse ReserveWeddingCheck(ReserveWeddingCheckRequest reserveWeddingCheckRequest);
        LoadInvitationResponse TryGetInvitationInfo(LoadInvitationRequest loadInvitationRequest);
        InviteGuestResponse TryInviteGuest(InviteGuestRequest inviteGuestRequest);
    }
}
