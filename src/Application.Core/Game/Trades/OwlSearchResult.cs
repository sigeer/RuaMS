namespace Application.Core.Game.Trades
{
    public class OwlSearchResult
    {
        public List<OwlSearchResultItem> Items { get; set; } = [];
    }

    public class OwlSearchResultItem
    {
        public PlayerShopItem Item { get; set; }
        public string Title { get; set; }
        public string OwnerName { get; set; }
        public int Channel { get; set; }
        public int MapId { get; set; }
        public int MapObjectId { get; set; }
    }
}
