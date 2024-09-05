using Application.Core.EF.Entities;
using AutoMapper;

namespace Application.Core
{
    public class GlobalConfigs
    {
        public static IMapper Mapper { get; set; } = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<GuildMapper>();
            cfg.AddProfile<CharacterMapper>();
            cfg.AddProfile<AllianceMapper>();
        }).CreateMapper();
    }
}
