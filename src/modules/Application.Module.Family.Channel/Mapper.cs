using AutoMapper;

namespace Application.Module.Family.Channel
{
    internal class Mapper : Profile
    {
        public Mapper()
        {
            CreateMap<Dto.FamilyDto, Models.Family>();
            CreateMap<Dto.FamilyMemberDto, Models.FamilyEntry>();
        }
    }
}
