using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using PlayerNPCProto;

namespace Application.Module.PlayerNPC.Master
{
    internal class GrpcServer : PlayerNPCProto.ChannelService.ChannelServiceBase
    {
        readonly PlayerNPCManager _manager;

        public GrpcServer(PlayerNPCManager manager)
        {
            _manager = manager;
        }

        public override Task<Empty> CreatePlayerNPC(CreatePlayerNPCRequest request, ServerCallContext context)
        {
            _manager.Create(request);
            return Task.FromResult(new Empty());
        }

        public override Task<CreatePlayerNPCPreResponse> CreatePlayerNPCCheck(CreatePlayerNPCPreRequest request, ServerCallContext context)
        {
            return Task.FromResult(_manager.PreCreate(request));
        }

        public override Task<GetMapPlayerNPCListResponse> GetMapPlayerNPC(GetMapPlayerNPCListRequest request, ServerCallContext context)
        {
            return Task.FromResult(_manager.GetMapData(request));
        }

        public override Task<GetAllPlayerNPCDataResponse> GetAllPlayerNPC(GetAllPlayerNPCDataRequest request, ServerCallContext context)
        {
            return Task.FromResult(_manager.GetAllData());
        }

        public override Task<Empty> RemoveAll(Empty request, ServerCallContext context)
        {
            _manager.RemoveAll();
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> RemoveByName(RemovePlayerNPCRequest request, ServerCallContext context)
        {
            _manager.Remove(request);
            return Task.FromResult(new Empty());
        }
    }
}
