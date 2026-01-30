using LifeProto;

namespace Application.Module.PlayerNPC.Channel.InProgress
{
    public class LocalChannelServerTransport : IChannelTransport
    {
        readonly Master.PlayerNPCManager _manager;

        public LocalChannelServerTransport(Master.PlayerNPCManager manager)
        {
            _manager = manager;
        }

        public void CreatePlayerNPC(CreatePlayerNPCRequest createRequest)
        {
            _manager.Create(createRequest);
        }

        public GetMapPlayerNPCListResponse GetMapPlayerNPCList(GetMapPlayerNPCListRequest getMapPlayerNPCListRequest)
        {
            return _manager.GetMapData(getMapPlayerNPCListRequest);
        }

        public GetAllPlayerNPCDataResponse GetAllPlayerNPCList()
        {
            return _manager.GetAllData();
        }

        public CreatePlayerNPCPreResponse PreCreatePlayerNPC(CreatePlayerNPCPreRequest commitPlayerNPCRequest)
        {
            return _manager.PreCreate(commitPlayerNPCRequest);
        }

        public void RemoveAllPlayerNPC()
        {
            _manager.RemoveAll();
        }

        public void RemovePlayerNPC(RemovePlayerNPCRequest removePlayerNPCRequest)
        {
            _manager.Remove(removePlayerNPCRequest);
        }
    }
}
