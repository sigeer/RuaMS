using Application.EF.Entities;

namespace Application.Core.Login.Mappers
{
    [Mapper]
    public interface IAccountMapper
    {
        Dto.AccountGameDto MapToDto(AccountEntity entity);
        AccountEntity MapToExisting(Dto.AccountGameDto dto, AccountEntity entity);

    }
}
