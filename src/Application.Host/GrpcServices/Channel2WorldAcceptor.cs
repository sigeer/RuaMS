using Application.Core.Game.TheWorld;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using net.server;
using RemoteService;

namespace Application.Host.GrpcServices
{
    public class Channel2WorldAcceptor : ServerCenterService.ServerCenterServiceBase
    {
        public override Task<AddChannelResult> AddChannel(ChannelInfo request, ServerCallContext context)
        {
            return Task.FromResult(new AddChannelResult
            {
                ChannelId = Server.getInstance().AddChannel(new RemoteWorldChannel(request.Id, new Shared.Servers.ActualServerConfig
                {
                    Host = request.Host,
                    Port = request.Port,
                    GrpcServiceEndPoint = request.GrpcAddress
                }))
            });
        }

        public override Task<Empty> RemoveChannel(ChannelInfo request, ServerCallContext context)
        {
            Server.getInstance().RemoveChannel(request.Id);
            return Task.FromResult(new Empty());
        }
    }
}
