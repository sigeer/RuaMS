using Application.EF.Entities;
using AutoMapper;

namespace Application.Module.Family.Master.Models
{
    internal class Mapper : Profile
    {
        public Mapper()
        {
            CreateMap<FamilyCharacterEntity, FamilyCharacterModel>()
                .ForMember(dest => dest.Id, src => src.MapFrom(x => x.Cid))
                .ReverseMap()
                .ForMember(dest => dest.Cid, src => src.MapFrom(x => x.Id));

            CreateMap<FamilyEntitlementEntity, FamilyEntitlementUseRecord>()
                .ForMember(dest => dest.Id, src => src.MapFrom(x => x.Entitlementid))
                .ForMember(dest => dest.Time, src => src.MapFrom(x => x.Timestamp));
        }
    }
}
