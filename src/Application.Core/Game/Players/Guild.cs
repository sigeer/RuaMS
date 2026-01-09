using AllianceProto;
using Application.Core.Game.Relation;
using GuildProto;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        public int getGuildId()
        {
            return GuildId;
        }

        public int getGuildRank()
        {
            return GuildRank;
        }

        public GuildDto? GetGuild()
        {
            if (GuildId <= 0)
            {
                return null;
            }
            return Client.CurrentServerContainer.GuildManager.GetGuild(GuildId);
        }

        public AllianceDto? GetAlliance()
        {
            var guild = GetGuild();
            if (guild == null)
                return null;
            return Client.CurrentServerContainer.GuildManager.GetAlliance(guild.AllianceId);
        }
    }
}
