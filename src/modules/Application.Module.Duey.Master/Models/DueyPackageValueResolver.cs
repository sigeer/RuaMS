using Application.Core.Login;
using Application.Shared.Constants;
using AutoMapper;
using DueyDto;

namespace Application.Module.Duey.Master.Models
{
    internal class DueyPackageValueResolver : IValueResolver<DueyPackageModel, DueyDto.DueyPackageDto, string>
    {
        readonly MasterServer _server;

        public DueyPackageValueResolver(MasterServer server)
        {
            _server = server;
        }

        public string Resolve(DueyPackageModel source, DueyPackageDto destination, string destMember, ResolutionContext context)
        {
            return _server.CharacterManager.FindPlayerById(source.SenderId)?.Character?.Name ?? StringConstants.CharacterUnknown;
        }
    }
}
