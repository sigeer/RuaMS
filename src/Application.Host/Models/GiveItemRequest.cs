using Application.Core.Login.Dtos.Item;

namespace Application.Host.Models
{
    public record GiveItemRequest(int[] Players, CreateItemRequestDto Item, string? Message);
}
