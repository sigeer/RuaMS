using Application.Core.Login.Models;
using Application.Core.Login.ServerData;
using Application.Shared.Constants;
using AutoMapper;
using Dto;

namespace Application.Core.Login.Mappers
{
    public class BuddyConverter : ITypeConverter<BuddyModel, BuddyProto.BuddyDto>
    {
        private readonly MasterServer _server;

        public BuddyConverter(MasterServer server)
        {
            _server = server;
        }

        public BuddyProto.BuddyDto Convert(BuddyModel source, BuddyProto.BuddyDto destination, ResolutionContext context)
        {
            var player = _server.CharacterManager.FindPlayerById(source.Id);
            if (player == null)
                return new BuddyProto.BuddyDto { Id = 0, Channel = -1, Group = source.Group, MapId = 0, Name = StringConstants.CharacterUnknown };

            return BuddyManager.GetChrBuddyDto(source.CharacterId, player, source.Group);
        }
    }
}
