using Application.Core.Login.Shared;

namespace Application.Core.Login.Models
{
    public class PlayerNpcModel : ITrackableEntityKey<int>
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public int Hair { get; set; }

        public int Face { get; set; }

        public int Skin { get; set; }

        public int Gender { get; set; }

        public int X { get; set; }

        public int Cy { get; set; }

        public int World { get; set; }

        public int Map { get; set; }

        public int Dir { get; set; }

        public int NpcId { get; set; }

        public int Fh { get; set; }

        public int Rx0 { get; set; }

        public int Rx1 { get; set; }

        public int Job { get; set; }
        public bool IsHonor { get; set; }
        public int OverallRank { get; set; }
        public int JobRank { get; set; }
        public List<PlayerNpcEquipModel> Equips { get; set; }
    }
}
