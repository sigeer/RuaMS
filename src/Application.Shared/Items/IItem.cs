namespace Application.Shared.Items
{
    public interface IItem
    {
        int ItemId { get; }
        short Quantity { get; set; }
        long UniqueId { get; }
        string? Properties { get; set; }
        long Expiration { get; }
        short Flag { get; set; }
    }
}
