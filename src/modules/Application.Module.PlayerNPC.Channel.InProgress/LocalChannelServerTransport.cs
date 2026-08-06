
namespace Application.Module.PlayerNPC.Channel.InProgress
{
    public class LocalChannelServerTransport : IChannelTransport
    {
        readonly Master.PlayerNPCManager _manager;

        public LocalChannelServerTransport(Master.PlayerNPCManager manager)
        {
            _manager = manager;
        }

        public void CreatePlayerNPC(ProtoService.CreatePlayerNPCRequest createRequest)
        {
            _manager.Create(createRequest);
        }

        public ProtoService.GetMapPlayerNPCListResponse GetMapPlayerNPCList(ProtoService.GetMapPlayerNPCListRequest getMapPlayerNPCListRequest)
        {
            return _manager.GetMapData(getMapPlayerNPCListRequest);
        }

        public ProtoService.GetAllPlayerNPCDataResponse GetAllPlayerNPCList()
        {
            return _manager.GetAllData();
        }

        public ProtoService.CreatePlayerNPCPreResponse PreCreatePlayerNPC(ProtoService.CreatePlayerNPCPreRequest commitPlayerNPCRequest)
        {
            return _manager.PreCreate(commitPlayerNPCRequest);
        }

        public void RemoveAllPlayerNPC()
        {
            _manager.RemoveAll();
        }

        public void RemovePlayerNPC(ProtoService.RemovePlayerNPCRequest removePlayerNPCRequest)
        {
            _manager.Remove(removePlayerNPCRequest);
        }
    }
}
