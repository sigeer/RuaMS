using AutoMapper;

namespace Application.Core.EF.Entities
{
    public class AccountMapper : Profile
    {
        public AccountMapper()
        {
            CreateMap<AccountEntity, Account>()
                .ForMember(x => x.LoginStage, opt => opt.MapFrom(x => x.Loggedin))
                .ForMember(x => x.LastLogin, opt => opt.MapFrom(x => x.Lastlogin));
        }
    }
}
