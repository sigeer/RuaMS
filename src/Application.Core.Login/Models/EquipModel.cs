namespace Application.Core.Login.Models
{
    public class EquipModel
    {
        public int Id { get; set; }
        public int InventoryItemId { get; set; }

        public int Upgradeslots { get; set; }

        public byte Level { get; set; }

        public int Str { get; set; }

        public int Dex { get; set; }

        public int Int { get; set; }

        public int Luk { get; set; }

        public int Hp { get; set; }

        public int Mp { get; set; }

        public int Watk { get; set; }

        public int Matk { get; set; }

        public int Wdef { get; set; }

        public int Mdef { get; set; }

        public int Acc { get; set; }

        public int Avoid { get; set; }

        public int Hands { get; set; }

        public int Speed { get; set; }

        public int Jump { get; set; }

        public int Locked { get; set; }

        public int Vicious { get; set; }

        public byte Itemlevel { get; set; }

        public int Itemexp { get; set; }
        public RingModel? RingInfo { get; set; }
    }
}
