using Application.Shared.Guild;

namespace Application.Core.Login.Models.Guilds
{
    public class AllianceModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public int Capacity { get; set; } = 2;

        public string Notice { get; set; } = "";

        public string Rank1 { get; set; } = null!;

        public string Rank2 { get; set; } = null!;

        public string Rank3 { get; set; } = null!;

        public string Rank4 { get; set; } = null!;

        public string Rank5 { get; set; } = null!;

        public List<int> Guilds { get; set; } = [];
        public bool TryAddGuild(int guild, out AllianceUpdateResult code)
        {
            code = 0;

            if (Guilds.Count >= Capacity)
            {
                code = AllianceUpdateResult.CapacityFull;
                return false;
            }

            if (Guilds.Contains(guild))
            {
                code = AllianceUpdateResult.AlreadyInAlliance;
                return false;
            }
                

            Guilds.Add(guild);
            return true;
        }

        public bool TryRemoveGuild(int guild, out AllianceUpdateResult code)
        {
            code = 0;

            return Guilds.Remove(guild);
        }
    }
}
