using Application.Core.Login.Models;

namespace Application.Core.Login.Mappers
{
    [Mapper]
    public interface ICharacterMapper
    {
        Dto.CharacterDto MapToDto(CharacterModel player);
        CharacterModel MapToExisting(Dto.CharacterDto dto, CharacterModel player);
    }
}
