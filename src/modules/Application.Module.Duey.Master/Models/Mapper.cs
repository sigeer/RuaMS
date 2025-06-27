using Application.EF.Entities;
using AutoMapper;

namespace Application.Module.Duey.Master.Models
{
    internal class Mapper : Profile
    {
        public Mapper()
        {
            CreateMap<DueyPackageEntity, DueyPackageModel>();
            CreateMap<DueyPackageModel, DueyDto.DueyPackageDto>()
                .ForMember(dest => dest.PackageId, src => src.MapFrom(x => x.Id))
                .ReverseMap();
        }
    }
}
