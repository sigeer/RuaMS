using Application.Core.Login.Models;

namespace Application.Core.Login.Mappers
{
    [Mapper]
    public interface IAccountMapper
    {
        AccountGame MapToObject(Dto.AccountGameDto dto);
    }
}
