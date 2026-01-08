using AllianceProto;
using Application.Core.Game.Relation;
using GuildProto;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        public PlayerGuildSnapshot? GuildSnapshot { get; set; }
        public PlayerAllianceSnapshot? AllianceSnapshot { get; set; }

        public PlayerAllianceSnapshot? getGuild() => AllianceSnapshot;

        public int getGuildId()
        {
            return GuildId;
        }

        public int getGuildRank()
        {
            return GuildRank;
        }

        public void SetGuildSnapshot(GuildDto? guild)
        {
            if (guild != null)
            {
                if (GuildSnapshot == null)
                {
                    GuildSnapshot = new PlayerGuildSnapshot(guild.GuildId, guild.Name, guild.AllianceId, guild.Logo, guild.LogoColor, guild.LogoBg, guild.LogoBgColor);
                }
                else
                {
                    GuildSnapshot.GuildId = guild.GuildId;
                    GuildSnapshot.GuildName = guild.Name;
                    GuildSnapshot.Logo = guild.Logo;
                    GuildSnapshot.LogoColor = guild.LogoColor;
                    GuildSnapshot.LogoBg = guild.LogoBg;
                    GuildSnapshot.LogoBgColor = guild.LogoBgColor;
                }
            }
        }

        public void RemoveGuildSnapshot()
        {
            GuildSnapshot = null;
            GuildRank = 5;
            RemoveAllianceSnapshot();
        }

        public void SetAllianceSnapshot(AllianceDto? alliance)
        {
            if (alliance != null)
            {
                if (AllianceSnapshot == null)
                {
                    AllianceSnapshot = new PlayerAllianceSnapshot(alliance.AllianceId, alliance.Name, alliance.Capacity);
                }
                else
                {
                    AllianceSnapshot.Id = alliance.AllianceId;
                    AllianceSnapshot.Name = alliance.Name;
                    AllianceSnapshot.Capacity = alliance.Capacity;
                }
            }
        }

        public void RemoveAllianceSnapshot()
        {
            AllianceSnapshot = null;
            AllianceRank = 5;
        }
    }
}
