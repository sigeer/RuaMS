using Grpc.Core;
using MarriageProto;

namespace Application.Module.Marriage.Master
{
    internal class GrpcService : MarriageServiceProto.ChannelService.ChannelServiceBase
    {
        readonly WeddingManager _weddingManager;
        readonly MarriageManager _marriageManager;

        public GrpcService(WeddingManager weddingManager, MarriageManager marriageManager)
        {
            _weddingManager = weddingManager;
            _marriageManager = marriageManager;
        }

        public override Task<CreateMarriageRelationResponse> CreateMarriageRelation(CreateMarriageRelationRequest request, ServerCallContext context)
        {
            return Task.FromResult(_marriageManager.CreateMarriageRelation(request));
        }

        public override Task<ReserveWeddingResponse> ReserveWedding(ReserveWeddingRequest request, ServerCallContext context)
        {
            return Task.FromResult(_weddingManager.ReserveWedding(request));
        }

        public override Task<CloseWeddingResponse> CloseWedding(CloseWeddingRequest request, ServerCallContext context)
        {
            _weddingManager.CloseWedding(request);
            return base.CloseWedding(request, context);
        }

        public override Task<CompleteWeddingResponse> CompleteWedding(CompleteWeddingRequest request, ServerCallContext context)
        {
            return Task.FromResult(_weddingManager.CompleteWedding(request));
        }

        public override Task<LoadInvitationResponse> LoadInvitationContent(LoadInvitationRequest request, ServerCallContext context)
        {
            return Task.FromResult(_weddingManager.GetInvitationContent(request));
        }

        public override Task<WeddingInfoListDto> LoadWeddings(LoadWeddingByIdRequest request, ServerCallContext context)
        {
            return Task.FromResult(_weddingManager.QueryWeddings(request));
        }

        public override Task<LoadMarriageInfoResponse> LoadEffectMarriageInfo(LoadMarriageInfoRequest request, ServerCallContext context)
        {
            return Task.FromResult(_marriageManager.GetEffectMarriageModelRemote(request));
        }
    }
}
