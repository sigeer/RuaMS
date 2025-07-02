using AutoMapper;

namespace Application.Module.Duey.Channel.Models
{
    internal class Mapper : Profile
    {
        public Mapper()
        {
            CreateMap<DueyDto.DueyPackageDto, DueyPackageObject>();
        }
    }
}
