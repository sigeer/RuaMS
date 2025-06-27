using Dto;
using DueyDto;

namespace Application.Module.Duey.Channel
{
    public interface IChannelTransport
    {
        void CreateDueyPackage(CreatePackageRequest request);
        void TakeDueyPackage(TakeDueyPackageRequest request);
        void RequestRemovePackage(RemovePackageRequest request);
        void TakeDueyPackageCommit(TakeDueyPackageCommit takeDueyPackageCommit);
        GetPlayerDueyPackageResponse GetDueyPackagesByPlayerId(GetPlayerDueyPackageRequest request);
    }
}
