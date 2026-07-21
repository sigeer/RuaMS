using Application.Core.Login.Models;

namespace Application.Core.Login.Mappers
{
    [Mapper]
    public interface IItemMapper
    {
        Dto.ItemDto MapToDto(ItemModel item);
        ItemModel MapToObject(Dto.ItemDto itemDto);
    }
}
