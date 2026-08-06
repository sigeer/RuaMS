
namespace Application.Module.PlayerNPC.Channel
{
    public interface IChannelTransport
    {
        ProtoService.CreatePlayerNPCPreResponse PreCreatePlayerNPC(ProtoService.CreatePlayerNPCPreRequest commitPlayerNPCRequest);
        void CreatePlayerNPC(ProtoService.CreatePlayerNPCRequest createRequest);
        void RemoveAllPlayerNPC();
        void RemovePlayerNPC(ProtoService.RemovePlayerNPCRequest removePlayerNPCRequest);
        ProtoService.GetMapPlayerNPCListResponse GetMapPlayerNPCList(ProtoService.GetMapPlayerNPCListRequest getMapPlayerNPCListRequest);
        ProtoService.GetAllPlayerNPCDataResponse GetAllPlayerNPCList();
    }
}
