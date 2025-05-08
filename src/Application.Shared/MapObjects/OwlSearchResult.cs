namespace Application.Shared.MapObjects
{
    public class OwlSearchResult
    {
        public int OwnerId { get; set; }
        public string OwnerName { get; set; }
        public int Channel { get; set; }
        public int MapId { get; set; }
        public string Description { get; set; }
        public int Bundles { get; set; }
        public int ItemQuantity { get; set; }
        public int Price { get; set; }
    }
}
