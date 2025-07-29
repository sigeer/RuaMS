using Application.Core.Login;
using Application.Shared.Constants;
using AutoMapper;

namespace Application.Module.Marriage.Master.Models
{
    internal class MarriageHusbandNameValueResolver : IValueResolver<MarriageModel, MarriageProto.MarriageDto, string>
    {
        readonly MasterServer _server;

        public MarriageHusbandNameValueResolver(MasterServer server)
        {
            _server = server;
        }

        public string Resolve(MarriageModel source, MarriageProto.MarriageDto destination, string destMember, ResolutionContext context)
        {
            return _server.CharacterManager.GetPlayerName(source.Husbandid);
        }
    }

    internal class MarriageWifeNameValueResolver : IValueResolver<MarriageModel, MarriageProto.MarriageDto, string>
    {
        readonly MasterServer _server;

        public MarriageWifeNameValueResolver(MasterServer server)
        {
            _server = server;
        }

        public string Resolve(MarriageModel source, MarriageProto.MarriageDto destination, string destMember, ResolutionContext context)
        {
            return _server.CharacterManager.GetPlayerName(source.Wifeid);
        }
    }
}
