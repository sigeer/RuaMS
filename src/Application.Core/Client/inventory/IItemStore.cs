namespace Application.Core.Client.inventory
{
    public interface IItemStore
    {
        ItemType StoreType { get; }
        Player Owner { get; }
    }
}
