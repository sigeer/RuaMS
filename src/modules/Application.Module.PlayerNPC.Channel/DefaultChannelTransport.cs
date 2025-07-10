using Application.Core.Channel;
using Grpc.Net.Client;
using net.server.services;
using PlayerNPCProto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Module.PlayerNPC.Channel
{
    public class DefaultChannelTransport : IChannelTransport
    {
        readonly PlayerNPCProto.ChannelService.ChannelServiceClient _grpcClient;
        public DefaultChannelTransport(WorldChannelServer server)
        {
            _grpcClient = new PlayerNPCProto.ChannelService.ChannelServiceClient(GrpcChannel.ForAddress(server.ServerConfig.MasterServerGrpcAddress));
        }

        public void CreatePlayerNPC(CreatePlayerNPCRequest request)
        {
            _grpcClient.CreatePlayerNPC(request);
        }

        public GetMapPlayerNPCListResponse GetMapPlayerNPCList(GetMapPlayerNPCListRequest request)
        {
            return _grpcClient.GetMapPlayerNPC(request);
        }

        public CreatePlayerNPCPreResponse PreCreatePlayerNPC(CreatePlayerNPCPreRequest request)
        {
            return _grpcClient.CreatePlayerNPCCheck(request);
        }

        public void RemoveAllPlayerNPC()
        {
            _grpcClient.RemoveAll(new Google.Protobuf.WellKnownTypes.Empty());
        }

        public void RemovePlayerNPC(RemovePlayerNPCRequest request)
        {
            _grpcClient.RemoveByName(request);
        }
    }
}
