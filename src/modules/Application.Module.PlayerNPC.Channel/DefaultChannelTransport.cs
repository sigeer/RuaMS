using LifeProto;

namespace Application.Module.PlayerNPC.Channel
{
    public class DefaultChannelTransport : IChannelTransport
    {
        readonly ServiceProto.GameService.GameServiceClient _grpcClient;
        public DefaultChannelTransport(ServiceProto.GameService.GameServiceClient client)
        {
            _grpcClient = client;
        }

        public void CreatePlayerNPC(CreatePlayerNPCRequest request)
        {
            _grpcClient.CreatePlayerNPC(request);
        }

        public GetMapPlayerNPCListResponse GetMapPlayerNPCList(GetMapPlayerNPCListRequest request)
        {
            return _grpcClient.GetMapPlayerNPC(request);
        }

        public GetAllPlayerNPCDataResponse GetAllPlayerNPCList()
        {
            return _grpcClient.GetAllPlayerNPC(new Google.Protobuf.WellKnownTypes.Empty());
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
