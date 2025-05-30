using Application.Core.EF.Entities.Items;
using Application.EF.Entities;
using AutoMapper;

namespace Application.Core.Login.Mappers
{
    /// <summary>
    /// entity -&gt; proto
    /// </summary>
    public class EntityMapper : Profile
    {
        public EntityMapper()
        {
            CreateMap<Ring_Entity, Dto.RingDto>();
            CreateMap<GiftRingPair, Dto.GiftDto>()
                .ForMember(x => x.Ring, src => src.MapFrom(x => x.Ring))
                .IncludeMembers(x => x.Gift);
            CreateMap<GiftEntity, Dto.GiftDto>();
        }
    }
}
