using Application.Core.Client.inventory;
using Application.Core.Mappers;
using client.inventory;
using Dto;

namespace Application.Core.Client.inventory
{
    public partial class ItemMapper : IItemMapper
    {
        public ItemDto MapToDto(Item p1)
        {
            return p1 == null ? null : new ItemDto()
            {
                Type = p1.PlayerInventory == null ? -1 : (int)p1.PlayerInventory.StoreType,
                Itemid = p1.getItemId(),
                InventoryType = (int)ProtoMapper.GetInventoryType(p1),
                Position = (int)p1.getPosition(),
                Quantity = (int)p1.getQuantity(),
                Owner = p1.getOwner(),
                Flag = (int)p1.getFlag(),
                Expiration = p1.getExpiration(),
                GiftFrom = p1.getGiftFrom(),
                UniqueId = p1.UniqueId,
                Properties = p1.Properties
            };
        }
        public Item MapToObject(ItemDto src)
        {
            return ProtoMapper.MapItem(src);
        }
    }
}