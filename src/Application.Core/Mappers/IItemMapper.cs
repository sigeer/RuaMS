using client.inventory;

namespace Application.Core.Mappers
{
    [Mapper]
    public interface IItemMapper
    {
        Dto.ItemDto MapToDto(Item? item);

        Item MapToObject(Dto.ItemDto itemDto);
    }
}
