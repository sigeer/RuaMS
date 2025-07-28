using Application.Module.Marriage.Master;
using MarriageProto;
using System;

namespace Application.Module.Marriage.Channel.InProgress
{
    public class LocalChannelTransport : IChannelServerTransport
    {
        readonly Master.MarriageManager _marriageManager;
        readonly Master.WeddingManager _weddingManager;

        public LocalChannelTransport(MarriageManager marriageManager, Master.WeddingManager weddingManager)
        {
            _marriageManager = marriageManager;
            _weddingManager = weddingManager;
        }

        public void BreakMarriage(BreakMarriageRequest breakMarriageRequest)
        {
            _marriageManager.BreakMarriage(breakMarriageRequest);
        }

        public void CloseWedding(CloseWeddingRequest closeWeddingRequest)
        {
            _weddingManager.CloseWedding(closeWeddingRequest);
        }

        public CompleteWeddingResponse CompleteWedding(CompleteWeddingRequest completeWeddingRequest)
        {
            return _weddingManager.CompleteWedding(completeWeddingRequest);
        }

        public CreateMarriageRelationResponse CreateMarriageRelation(CreateMarriageRelationRequest createMarriageRelationRequest)
        {
            return _marriageManager.CreateMarriageRelation(createMarriageRelationRequest);
        }

        public WeddingInfoListDto LoadAllWeddingById(LoadWeddingByIdRequest request)
        {
            return _weddingManager.QueryWeddings(request);
        }

        public object LoadMarriageGifts(LoadMarriageGiftsRequest loadMarriageGiftsRequest)
        {
            throw new NotImplementedException();
        }

        public ReserveWeddingResponse ReserveWedding(ReserveWeddingRequest reserveWeddingRequest)
        {
            return _weddingManager.ReserveWedding(reserveWeddingRequest);
        }

        public LoadInvitationResponse TryGetInvitationInfo(LoadInvitationRequest loadInvitationRequest)
        {
            return _weddingManager.GetInvitationContent(loadInvitationRequest);
        }

        public InviteGuestResponse TryInviteGuest(InviteGuestRequest inviteGuestRequest)
        {
            return _weddingManager.InviteGuest(inviteGuestRequest);
        }
    }
}
