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
        MarriageProto.ReserveWeddingResponse ReserveWedding(ReserveWeddingRequest reserveWeddingRequest);
        LoadInvitationResponse TryGetInvitationInfo(LoadInvitationRequest loadInvitationRequest);
        InviteGuestResponse TryInviteGuest(InviteGuestRequest inviteGuestRequest);
    }
}
