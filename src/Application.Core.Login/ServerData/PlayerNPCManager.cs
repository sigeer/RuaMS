using Application.Utility.Exceptions;
using LifeProto;

namespace Application.Core.Login.ServerData
{
    public interface IPlayerNPCManager
    {
        GetMapPlayerNPCListResponse GetMapData(GetMapPlayerNPCListRequest request);
        void Remove(RemovePlayerNPCRequest request);
        void RemoveAll();
        CreatePlayerNPCPreResponse PreCreate(CreatePlayerNPCPreRequest request);
        void Create(CreatePlayerNPCRequest request);
        GetAllPlayerNPCDataResponse GetAllData();
    }

    public class DefaultPlayerNPCManager : IPlayerNPCManager
    {
        public void Create(CreatePlayerNPCRequest request)
        {
            throw new BusinessNotsupportException();
        }

        public GetAllPlayerNPCDataResponse GetAllData()
        {
            return new GetAllPlayerNPCDataResponse();
        }

        public GetMapPlayerNPCListResponse GetMapData(GetMapPlayerNPCListRequest request)
        {
            return new GetMapPlayerNPCListResponse();
        }

        public CreatePlayerNPCPreResponse PreCreate(CreatePlayerNPCPreRequest request)
        {
            throw new BusinessNotsupportException();
        }

        public void Remove(RemovePlayerNPCRequest request)
        {

        }

        public void RemoveAll()
        {

        }
    }
}
