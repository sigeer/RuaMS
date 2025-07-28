using Application.Core.Login;
using Application.Shared.Constants;
using AutoMapper;

namespace Application.Module.Marriage.Master.Models
{
    internal class WeddingBrideNameValueResolver : IValueResolver<WeddingInfo, MarriageProto.WeddingInfoDto, string>
    {
        readonly MasterServer _server;

        public WeddingBrideNameValueResolver(MasterServer server)
        {
            _server = server;
        }

        public string Resolve(WeddingInfo source, MarriageProto.WeddingInfoDto destination, string destMember, ResolutionContext context)
        {
            return _server.CharacterManager.GetPlayerName(source.BrideId);
        }
    }

    internal class WeddingGroomNameValueResolver : IValueResolver<WeddingInfo, MarriageProto.WeddingInfoDto, string>
    {
        readonly MasterServer _server;

        public WeddingGroomNameValueResolver(MasterServer server)
        {
            _server = server;
        }

        public string Resolve(WeddingInfo source, MarriageProto.WeddingInfoDto destination, string destMember, ResolutionContext context)
        {
            return _server.CharacterManager.GetPlayerName(source.GroomId);
        }
    }
}
