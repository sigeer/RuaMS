using Application.Shared.Relations;
using AutoMapper;

namespace Application.Core.Mappers
{
    public class TeamMapper : Profile
    {
        public TeamMapper()
        {
            CreateMap<IPlayer, TeamMember>();
        }
    }
}
