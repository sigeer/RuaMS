using Application.Core.Login.Models;
using Application.EF.Entities;

namespace Application.Core.Login.Mappers
{
    [Mapper]
    public interface ICharacterMapper
    {
        ProtoModel.CharacterProto MapToDto(CharacterEntity player);
        CharacterEntity MapToExisting(ProtoModel.CharacterProto dto, CharacterEntity player);
    }
}
