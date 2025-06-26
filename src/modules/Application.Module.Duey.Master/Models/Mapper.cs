using AutoMapper;

namespace Application.Module.Duey.Master.Models
{
    internal class Mapper : Profile
    {
        public Mapper()
        {
            CreateMap<DueyPackageModel, DueyDto.DueyPackageDto>().ReverseMap();
        }
    }
}
