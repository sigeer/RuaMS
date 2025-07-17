using Application.Core.Login.Shared;

namespace Application.Core.Login.Models.Items
{
    public class PlayerShopRegistry : ITrackableEntityKey<int>
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public int Channel { get; set; }
        public int MapId { get; set; }

        public List<PlayerShopItemModel> Items { get; set; } = [];
        public int Meso { get; set; }

        public int Daynotes { get; set; }

        public long UpdateTime { get; set; }
    }
}
