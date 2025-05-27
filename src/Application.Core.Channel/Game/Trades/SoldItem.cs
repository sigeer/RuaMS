namespace Application.Core.Game.Trades
{
    public record SoldItem(string buyer, int itemid, short quantity, int mesos);
}
