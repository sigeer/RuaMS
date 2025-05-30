using Application.Core.Mappers;
using AutoMapper;
using System.Text;

namespace Application.Core
{
    public class GlobalTools
    {
        public static IMapper Mapper { get; set; } = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<OldEntityMapper>();
        }).CreateMapper();

    }
}
