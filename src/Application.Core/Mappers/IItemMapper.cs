using client.inventory;

namespace Application.Core.Mappers
{
    [Mapper]
    public interface IItemMapper
    {
        ProtoModel.ItemProto MapToDto(Item? item);

        Item MapToObject(ProtoModel.ItemProto itemDto);
    }
}
