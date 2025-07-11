using Application.Module.Duey.Master;
using AutoMapper;
using DueyDto;

namespace Application.Module.Duey.Channel.InProgress
{
    public class LocalDueyChannelTransport : IChannelTransport
    {
        readonly Application.Module.Duey.Master.DueyManager _masterManager;
        readonly IMapper _mapper;

        public LocalDueyChannelTransport(Master.DueyManager masterManager, IMapper mapper)
        {
            _masterManager = masterManager;
            _mapper = mapper;
        }

        public void CreateDueyPackage(CreatePackageRequest request)
        {
            _masterManager.CreateDueyPackage(request);
        }

        public GetPlayerDueyPackageResponse GetDueyPackagesByPlayerId(GetPlayerDueyPackageRequest request)
        {
            return _masterManager.GetPlayerDueyPackages(request);
        }

        public void RequestRemovePackage(RemovePackageRequest request)
        {
            _masterManager.RemovePackage(request);
        }

        public void TakeDueyPackage(TakeDueyPackageRequest request)
        {
            _masterManager.TakeDueyPackage(request);
        }

        public void TakeDueyPackageCommit(TakeDueyPackageCommit request)
        {
            _masterManager.TakeDueyPackageCommit(request);
        }
    }
}
