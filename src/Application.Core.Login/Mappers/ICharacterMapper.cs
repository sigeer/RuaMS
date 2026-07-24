using Application.Core.Login.Models;
using Application.EF.Entities;

namespace Application.Core.Login.Mappers
{
    [Mapper]
    public interface ICharacterMapper
    {
        Dto.CharacterDto MapToDto(CharacterEntity player);
        CharacterEntity MapToExisting(Dto.CharacterDto dto, CharacterEntity player);
    }
}
