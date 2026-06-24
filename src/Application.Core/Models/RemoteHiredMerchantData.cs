using client.inventory;

namespace Application.Core.Models
{
    public class RemoteHiredMerchantData
    {
        public int FeeMeso { get; set; }
        public int FeePercentage { get; set; }
        public int Meso { get; set; }
        public Item[] Items { get; set; } = [];

        public int Channel { get; set; }
        public int MapId { get; set; }
        public bool HasItem => Items.Length > 0 || Meso != 0;
    }
}
