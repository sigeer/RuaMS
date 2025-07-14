using Application.Core.Login.Models;
using Application.Shared.Constants;
using AutoMapper;
using ItemProto;

namespace Application.Core.Login.Mappers
{
    public class RingCharacterName1ValueResolver : IValueResolver<RingSourceModel, ItemProto.RingDto, string>
    {
        readonly MasterServer _server;

        public RingCharacterName1ValueResolver(MasterServer server)
        {
            _server = server;
        }

        public string Resolve(RingSourceModel source, RingDto destination, string destMember, ResolutionContext context)
        {
            return _server.CharacterManager.FindPlayerById(source.CharacterId1)?.Character?.Name ?? StringConstants.CharacterUnknown;
        }
    }

    public class RingCharacterName2ValueResolver : IValueResolver<RingSourceModel, ItemProto.RingDto, string>
    {
        readonly MasterServer _server;

        public RingCharacterName2ValueResolver(MasterServer server)
        {
            _server = server;
        }

        public string Resolve(RingSourceModel source, RingDto destination, string destMember, ResolutionContext context)
        {
            return _server.CharacterManager.FindPlayerById(source.CharacterId2)?.Character?.Name ?? StringConstants.CharacterUnknown;
        }
    }
}
