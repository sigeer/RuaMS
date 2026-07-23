namespace Application.Core.Mappers
{
    [Mapper]
    public interface IPlayerMapper
    {
        Dto.CharacterDto MapToDto(Player item);

        Player MapToExisting(Dto.CharacterDto dto, Player player);
    }
}
