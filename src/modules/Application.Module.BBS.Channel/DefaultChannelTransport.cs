using Application.Core.Channel;
using BBSProto;
using Grpc.Net.Client;

namespace Application.Module.BBS.Channel
{
    internal class DefaultChannelTransport : IChannelTransport
    {
        readonly BBSService.ChannelService.ChannelServiceClient _grpcClient;
        public DefaultChannelTransport(WorldChannelServer server)
        {
            _grpcClient = new BBSService.ChannelService.ChannelServiceClient(GrpcChannel.ForAddress(server.ServerConfig.MasterServerGrpcAddress));
        }
        public ShowBBSMainThreadResponse DeleteReply(DeleteReplyRequest deleteReplyRequest)
        {
            return _grpcClient.DeleteReply(deleteReplyRequest);
        }

        public void DeleteThread(DeleteThreadRequest deleteThreadRequest)
        {
            _grpcClient.DeleteThread(deleteThreadRequest);
        }

        public ShowBBSMainThreadResponse EditThread(PostThreadRequest postThreadRequest)
        {
            return _grpcClient.EditThread(postThreadRequest);
        }

        public ListBBSResponse ListThreads(ListBBSRequest listBBSRequest)
        {
            return _grpcClient.ListThreads(listBBSRequest);
        }

        public ShowBBSMainThreadResponse PostReply(PostReplyRequest postReplyRequest)
        {
            return _grpcClient.PostReply(postReplyRequest);
        }

        public ShowBBSMainThreadResponse ShowThread(ShowThreadRequest showThreadRequest)
        {
            return _grpcClient.ShowThread(showThreadRequest);
        }
    }
}
