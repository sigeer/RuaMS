using Application.EF.Entities;
using AutoMapper;

namespace Application.Module.Duey.Master.Models
{
    internal class Mapper : Profile
    {
        public Mapper()
        {
            CreateMap<DueyPackageEntity, DueyPackageModel>()
                .ForMember(dest => dest.Id, src => src.MapFrom(x => x.PackageId))
                .ReverseMap()
                .ForMember(dest => dest.PackageId, src => src.MapFrom(x => x.Id));
            CreateMap<DueyPackageModel, DueyDto.DueyPackageDto>()
                .ForMember(dest => dest.PackageId, src => src.MapFrom(x => x.Id))
                .ForMember(dest => dest.SenderName, src => src.MapFrom<DueyPackageValueResolver>());
        }
    }
}
