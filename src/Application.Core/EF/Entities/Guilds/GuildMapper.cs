using Application.Core.Game.Relation;
using AutoMapper;

namespace Application.Core.EF.Entities
{
    public class GuildMapper : Profile
    {
        public GuildMapper()
        {
            CreateMap<GuildEntity, Guild>().ReverseMap();
        }
    }
}
