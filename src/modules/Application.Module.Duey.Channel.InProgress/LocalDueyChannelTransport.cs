using Application.Module.Duey.Master;

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

        public ProtoService.CreatePackageResponse CreateDueyPackage(ProtoService.CreatePackageRequest request)
        {
            return _masterManager.CreateDueyPackage(request);
        }

        public ProtoService.GetPlayerDueyPackageResponse GetDueyPackagesByPlayerId(ProtoService.GetPlayerDueyPackageRequest request)
        {
            return _masterManager.GetPlayerDueyPackages(request);
        }

        public void RequestRemovePackage(ProtoService.RemovePackageRequest request)
        {
            _masterManager.RemovePackage(request);
        }

        public void TakeDueyPackage(ProtoService.TakeDueyPackageRequest request)
        {
            _masterManager.TakeDueyPackage(request);
        }

        public void TakeDueyPackageCommit(ProtoModel.TakeDueyPackageCommitProto request)
        {
            _masterManager.TakeDueyPackageCommit(request);
        }
    }
}
