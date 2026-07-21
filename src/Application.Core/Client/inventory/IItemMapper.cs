using client.inventory;

namespace Application.Core.Client.inventory
{
    [Mapper]
    public interface IItemMapper
    {
        Dto.ItemDto MapToDto(Item item);

        Item MapToObject(Dto.ItemDto itemDto);
    }
}
