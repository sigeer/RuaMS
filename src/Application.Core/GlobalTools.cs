using Application.Core.EF.Entities;
using Application.Core.EF.Entities.Items;
using Application.Core.Game.Maps;
using Application.Core.Game.Maps.Specials;
using AutoMapper;
using System.Text;

namespace Application.Core
{
    public class GlobalTools
    {
        public static IMapper Mapper { get; set; } = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MapCopyMapper>();
            cfg.AddProfile<GuildMapper>();
            cfg.AddProfile<CharacterMapper>();
            cfg.AddProfile<AllianceMapper>();
            cfg.AddProfile<ItemMapper>();
        }).CreateMapper();

        public static Encoding Encoding { get; set; } = null!;
    }

    public class MapCopyMapper: Profile
    {
        public MapCopyMapper()
        {
            CreateMap<MapleMap, MapleMap>();
            CreateMap<MonsterCarnivalMap, MonsterCarnivalMap>()
                .ConstructUsing(src => new MonsterCarnivalMap(src));
        }
    }
}
