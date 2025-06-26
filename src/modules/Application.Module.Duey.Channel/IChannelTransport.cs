using Dto;
using DueyDto;

namespace Application.Module.Duey.Channel
{
    public interface IChannelTransport
    {
        void CreateDueyPackage(CreatePackageRequest request);
        DueyDto.TakeDueyPackageResponse TakeDueyPackage(TakeDueyPackageRequest request);
        void RequestRemovePackage(RemovePackageRequest request);
        void TakeDueyPackageCommit(TakeDueyPackageCommit takeDueyPackageCommit);
        GetPlayerDueyPackageResponse GetDueyPackagesByPlayerId(GetPlayerDueyPackageRequest request);
    }
}
