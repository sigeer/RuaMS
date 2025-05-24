using Application.Core.EF.Entities;
using Application.Core.EF.Entities.Items;
using AutoMapper;
using System.Text;

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

        public static Encoding Encoding { get; set; } = null!;
    }
}
