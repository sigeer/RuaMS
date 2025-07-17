using client.inventory;

namespace Application.Core.Models
{
    public class PlayerShopLocalInfo
    {
        public int Mesos { get; set; }
        public Item[] Items { get; set; } = [];

        public int Channel { get; set; }
        public int MapId { get; set; }

        public bool HasItem => Items.Length > 0 || Mesos != 0;
    }
}
