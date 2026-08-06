
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

        public ProtoModel.GuildProto? GetGuild()
        {
            if (GuildId <= 0)
            {
                return null;
            }
            return Client.CurrentServer.NodeService.GuildManager.GetGuild(GuildId);
        }

        public ProtoModel.AllianceProto? GetAlliance()
        {
            var guild = GetGuild();
            if (guild == null)
                return null;
            return Client.CurrentServer.NodeService.GuildManager.GetAlliance(guild.AllianceId);
        }
    }
}
