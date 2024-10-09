namespace client.inventory
{
    public record ItemMessage(int Id, string Message);
    public record ItemMessagePair(Item Item, string? Message);
}
