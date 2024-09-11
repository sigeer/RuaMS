using Application.Core.Game.Items;
using AutoMapper;

namespace Application.Core.EF.Entities.Items
{
    public class ItemMapper : Profile
    {
        public ItemMapper()
        {
            CreateMap<PetEntity, Pet>()
                .ForMember(x => x.Fullness, opt => opt.MapFrom(x => Math.Min(Pet.MaxFullness, x.Fullness)))
                .ForMember(x => x.Level, opt => opt.MapFrom(x => Math.Min(Pet.MaxLevel, x.Level)))
                .ForMember(x => x.Tameness, opt => opt.MapFrom(x => Math.Min(Pet.MaxTameness, x.Closeness)))
                .ReverseMap()
                .ForMember(x => x.Fullness, opt => opt.MapFrom(x => Math.Min(Pet.MaxFullness, x.Fullness)))
                .ForMember(x => x.Level, opt => opt.MapFrom(x => Math.Min(Pet.MaxLevel, (int)x.Level)))
                .ForMember(x => x.Closeness, opt => opt.MapFrom(x => Math.Min(Pet.MaxTameness, x.Tameness)));
        }
    }
}
