using Application.Core.Login.Dtos.Item;
using Application.Shared.Items;

namespace Application.Host.Models
{
    public record GiveItemRequest(int[] Players, CreateItemRequestDto Item, string? Message);
}
