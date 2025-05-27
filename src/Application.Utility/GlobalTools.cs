using System.Text;

namespace Application.Utility
{
    public class GlobalTools
    {
        //public static IMapper Mapper { get; set; } = new MapperConfiguration(cfg =>
        //{
        //    cfg.AddProfile<GuildMapper>();
        //    cfg.AddProfile<CharacterMapper>();
        //    cfg.AddProfile<AllianceMapper>();
        //    cfg.AddProfile<ItemMapper>();
        //}).CreateMapper();

        public static Encoding Encoding { get; set; } = null!;
    }
}
