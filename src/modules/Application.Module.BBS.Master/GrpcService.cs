using BBSProto;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Application.Module.BBS.Master
{
    internal class GrpcService : BBSService.ChannelService.ChannelServiceBase
    {
        readonly BBSManager _manager;

        public GrpcService(BBSManager manager)
        {
            _manager = manager;
        }

        public override async Task<ShowBBSMainThreadResponse> DeleteReply(DeleteReplyRequest request, ServerCallContext context)
        {
            await base.DeleteReply(request, context);
            return _manager.DeleteBBSReply(request);
        }
        public override Task<Empty> DeleteThread(DeleteThreadRequest request, ServerCallContext context)
        {
            _manager.DeleteThread(request);
            return Task.FromResult(new Empty());
        }

        public override async Task<ShowBBSMainThreadResponse> EditThread(PostThreadRequest request, ServerCallContext context)
        {
            await base.EditThread(request, context);
            return _manager.EditThread(request);
        }

        public override async Task<ListBBSResponse> ListThreads(ListBBSRequest request, ServerCallContext context)
        {
            await base.ListThreads(request, context);
            return _manager.ListBBSThreads(request);
        }

        public override async Task<ShowBBSMainThreadResponse> PostReply(PostReplyRequest request, ServerCallContext context)
        {
            await base.PostReply(request, context);
            return _manager.PostReply(request);
        }

        public override async Task<ShowBBSMainThreadResponse> ShowThread(ShowThreadRequest request, ServerCallContext context)
        {
            await base.ShowThread(request, context);
            return _manager.ShowThread(request);
        }
    }
}
