using Application.Core.Login.Models;
using Application.Shared.Constants;
using AutoMapper;
using Dto;

namespace Application.Core.Login.Mappers
{
    public class BuddyConverter : ITypeConverter<BuddyModel, BuddyDto>
    {
        private readonly MasterServer _server;

        public BuddyConverter(MasterServer server)
        {
            _server = server;
        }

        public BuddyDto Convert(BuddyModel source, BuddyDto destination, ResolutionContext context)
        {
            var player = _server.CharacterManager.FindPlayerById(source.Id);

            return new BuddyDto
            {
                Id = source.Id,
                Name = player?.Character?.Name ?? StringConstants.CharacterUnknown,
                Channel = player?.Channel ?? 0,
                MapId = player?.Character?.Map ?? 0,
                Group = source.Group
            };
        }
    }
}
