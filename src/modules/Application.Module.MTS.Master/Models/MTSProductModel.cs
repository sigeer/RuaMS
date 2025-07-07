using Application.Core.Login.Models;
using Org.BouncyCastle.Asn1.Mozilla;

namespace Application.Module.MTS.Master.Models
{
    public class MTSProductModel
    {
        public int Id { get; set; }
        public int OwnerId { get; set; }
        public ItemModel Item { get; set; } = null!;
        public bool IsInTransfer { get; set; }
        public int Tab { get; set; }
        public int Type { get; set; }
        public int Price { get; set; }
    }
}
