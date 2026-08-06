
namespace Application.Module.PlayerNPC.Channel
{
    public class DefaultChannelTransport : IChannelTransport
    {
        readonly ProtoService.GameService.GameServiceClient _grpcClient;
        public DefaultChannelTransport(ProtoService.GameService.GameServiceClient client)
        {
            _grpcClient = client;
        }

        public void CreatePlayerNPC(ProtoService.CreatePlayerNPCRequest request)
        {
            _grpcClient.CreatePlayerNPC(request);
        }

        public ProtoService.GetMapPlayerNPCListResponse GetMapPlayerNPCList(ProtoService.GetMapPlayerNPCListRequest request)
        {
            return _grpcClient.GetMapPlayerNPC(request);
        }

        public ProtoService.GetAllPlayerNPCDataResponse GetAllPlayerNPCList()
        {
            return _grpcClient.GetAllPlayerNPC(new Google.Protobuf.WellKnownTypes.Empty());
        }

        public ProtoService.CreatePlayerNPCPreResponse PreCreatePlayerNPC(ProtoService.CreatePlayerNPCPreRequest request)
        {
            return _grpcClient.CreatePlayerNPCCheck(request);
        }

        public void RemoveAllPlayerNPC()
        {
            _grpcClient.RemoveAll(new Google.Protobuf.WellKnownTypes.Empty());
        }

        public void RemovePlayerNPC(ProtoService.RemovePlayerNPCRequest request)
        {
            _grpcClient.RemoveByName(request);
        }
    }
}
