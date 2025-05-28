using AutoMapper;
using Rank;

namespace Application.Core.Mappers
{
    /// <summary>
    /// Proto -&gt; Object
    /// </summary>
    public class Proto2ObjectMapper : Profile
    {
        public Proto2ObjectMapper()
        {
            CreateMap<Rank.RankCharacter, RankedCharacterInfo>()
                .ForMember(dest => dest.Rank, src => src.MapFrom(x => x.Rank))
                .ForMember(dest => dest.CharacterName, src => src.MapFrom(x => x.Name))
                .ForMember(dest => dest.CharacterLevel, src => src.MapFrom(x => x.Level));
        }
    }
}
