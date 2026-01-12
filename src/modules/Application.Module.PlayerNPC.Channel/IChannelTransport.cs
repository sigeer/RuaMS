using LifeProto;

namespace Application.Module.PlayerNPC.Channel
{
    public interface IChannelTransport
    {
        CreatePlayerNPCPreResponse PreCreatePlayerNPC(CreatePlayerNPCPreRequest commitPlayerNPCRequest);
        void CreatePlayerNPC(CreatePlayerNPCRequest createRequest);
        void RemoveAllPlayerNPC();
        void RemovePlayerNPC(RemovePlayerNPCRequest removePlayerNPCRequest);
        GetMapPlayerNPCListResponse GetMapPlayerNPCList(GetMapPlayerNPCListRequest getMapPlayerNPCListRequest);
        GetAllPlayerNPCDataResponse GetAllPlayerNPCList();
    }
}
