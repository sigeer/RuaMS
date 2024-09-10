using Application.Core.EF.Entities;
using Application.Core.EF.Entities.Items;
using AutoMapper;

namespace Application.Core
{
    public class GlobalTools
    {
        public static IMapper Mapper { get; set; } = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<GuildMapper>();
            cfg.AddProfile<CharacterMapper>();
            cfg.AddProfile<AllianceMapper>();
            cfg.AddProfile<ItemMapper>();
        }).CreateMapper();
    }
}
