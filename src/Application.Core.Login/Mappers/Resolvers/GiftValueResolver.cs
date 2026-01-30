using Application.Core.Login.Models;
using Application.Shared.Constants;
using AutoMapper;
using ItemProto;

namespace Application.Core.Login.Mappers.Resolvers
{
    public class GiftFromNameValueResolver : IValueResolver<GiftModel, ItemProto.GiftDto, string>
    {
        readonly MasterServer _server;

        public GiftFromNameValueResolver(MasterServer server)
        {
            _server = server;
        }

        public string Resolve(GiftModel source, GiftDto destination, string destMember, ResolutionContext context)
        {
            return _server.CharacterManager.FindPlayerById(source.From)?.Character?.Name ?? StringConstants.CharacterUnknown;
        }
    }

    public class GiftToNameValueResolver : IValueResolver<GiftModel, ItemProto.GiftDto, string>
    {
        readonly MasterServer _server;

        public GiftToNameValueResolver(MasterServer server)
        {
            _server = server;
        }

        public string Resolve(GiftModel source, GiftDto destination, string destMember, ResolutionContext context)
        {
            return _server.CharacterManager.FindPlayerById(source.To)?.Character?.Name ?? StringConstants.CharacterUnknown;
        }
    }
}
