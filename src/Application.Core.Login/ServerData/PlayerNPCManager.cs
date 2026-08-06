using Application.Utility.Exceptions;

namespace Application.Core.Login.ServerData
{
    public interface IPlayerNPCManager
    {
        ProtoService.GetMapPlayerNPCListResponse GetMapData(ProtoService.GetMapPlayerNPCListRequest request);
        void Remove(ProtoService.RemovePlayerNPCRequest request);
        void RemoveAll();
        ProtoService.CreatePlayerNPCPreResponse PreCreate(ProtoService.CreatePlayerNPCPreRequest request);
        void Create(ProtoService.CreatePlayerNPCRequest request);
        ProtoService.GetAllPlayerNPCDataResponse GetAllData();
    }

    public class DefaultPlayerNPCManager : IPlayerNPCManager
    {
        public void Create(ProtoService.CreatePlayerNPCRequest request)
        {
            throw new BusinessNotsupportException();
        }

        public ProtoService.GetAllPlayerNPCDataResponse GetAllData()
        {
            return new ProtoService.GetAllPlayerNPCDataResponse();
        }

        public ProtoService.GetMapPlayerNPCListResponse GetMapData(ProtoService.GetMapPlayerNPCListRequest request)
        {
            return new ProtoService.GetMapPlayerNPCListResponse();
        }

        public ProtoService.CreatePlayerNPCPreResponse PreCreate(ProtoService.CreatePlayerNPCPreRequest request)
        {
            throw new BusinessNotsupportException();
        }

        public void Remove(ProtoService.RemovePlayerNPCRequest request)
        {

        }

        public void RemoveAll()
        {

        }
    }
}
