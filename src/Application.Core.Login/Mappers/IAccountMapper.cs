using Application.EF.Entities;

namespace Application.Core.Login.Mappers
{
    [Mapper]
    public interface IAccountMapper
    {
        ProtoModel.AccountGameProto MapToDto(AccountEntity entity);
        AccountEntity MapToExisting(ProtoModel.AccountGameProto dto, AccountEntity entity);

    }
}
