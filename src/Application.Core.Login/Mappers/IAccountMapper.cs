using Application.EF.Entities;

namespace Application.Core.Login.Mappers
{
    [Mapper]
    public interface IAccountMapper
    {
        AccountDto.AccountGameDto MapToDto(AccountEntity entity);
        AccountEntity MapToExisting(AccountDto.AccountGameDto dto, AccountEntity entity);

    }
}
