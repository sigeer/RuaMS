using Application.Core.Game.Relation;
using AutoMapper;

namespace Application.Core.EF.Entities
{
    public class AllianceMapper : Profile
    {
        public AllianceMapper()
        {
            CreateMap<AllianceEntity, Alliance>()
                .ForMember(x => x.RankTitles, opt => opt.MapFrom(x => new string[] { x.Rank1, x.Rank2, x.Rank3, x.Rank4, x.Rank5 }))
                .ForMember(x => x.AllianceId, opt => opt.MapFrom(x => x.Id))
                .ReverseMap();
        }
    }
}
