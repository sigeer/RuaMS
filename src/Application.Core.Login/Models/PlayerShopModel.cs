using System.Drawing;

namespace Application.Core.Login.Models
{
    public class PlayerShopModel
    {
        public int ItemId { get; set; }
        public int CharacterId { get; set; }
        public string Description { get; set; } = null!;
        public int Channel { get; set; }
        public int MapId { get; set; }
        public Point Position { get; set; }
    }

    public class MerchantShopModel : PlayerShopModel
    {
        public DateTimeOffset ExpiredAt { get; set; }
    }
}
