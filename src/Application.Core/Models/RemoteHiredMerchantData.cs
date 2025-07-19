using client.inventory;

namespace Application.Core.Models
{
    public class RemoteHiredMerchantData
    {
        public int Mesos { get; set; }
        public Item[] Items { get; set; } = [];

        public int Channel { get; set; }
        public string MapName { get; set; }

        public bool HasItem => Items.Length > 0 || Mesos != 0;
    }
}
