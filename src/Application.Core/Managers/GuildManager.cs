using Application.Core.Game.Invites;
using Application.Core.Game.Relation;
using net.server;
using net.server.coordinator.matchchecker;
using net.server.guild;

namespace Application.Core.Managers
{
    public class GuildManager
    {
        readonly static ILogger _log = LogFactory.GetLogger(LogType.Guild);


        public static void displayGuildRanks(IChannelClient c, int npcid)
        {
            try
            {
                using var dbContext = new DBContext();
                var rs = dbContext.Guilds.OrderByDescending(x => x.GP).Take(50).ToList();
                c.sendPacket(GuildPackets.showGuildRanks(npcid, rs));
            }
            catch (Exception e)
            {
                _log.Error(e, "Failed to display guild ranks.");
            }
        }

        public static int getIncreaseGuildCost(int size)
        {
            int cost = YamlConfig.config.server.EXPAND_GUILD_BASE_COST + Math.Max(0, (size - 15) / 5) * YamlConfig.config.server.EXPAND_GUILD_TIER_COST;

            if (size > 30)
            {
                return Math.Min(YamlConfig.config.server.EXPAND_GUILD_MAX_COST, Math.Max(cost, 5000000));
            }
            else
            {
                return cost;
            }
        }
    }
}
