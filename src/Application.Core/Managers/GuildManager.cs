using Application.Core.Client;
using Application.Core.Game.Invites;
using Application.Core.Game.Relation;
using Microsoft.EntityFrameworkCore;
using net.server;
using net.server.coordinator.matchchecker;
using net.server.coordinator.world;
using net.server.guild;

namespace Application.Core.Managers
{
    public class GuildManager
    {
        readonly static ILogger _log = LogFactory.GetLogger(LogType.Guild);

        public static bool CheckGuildName(string name)
        {
            if (name.Length < 3 || name.Length > 12)
            {
                return false;
            }
            for (int i = 0; i < name.Length; i++)
            {
                if (!char.IsLower(name.ElementAt(i)) && !char.IsUpper(name.ElementAt(i)))
                {
                    return false;
                }
            }
            return true;
        }
        public static int CreateGuild(string name, int leaderId)
        {
            using var dbContext = new DBContext();
            if (dbContext.Guilds.Any(x => x.Name == name))
                return 0;

            using var transaction = dbContext.Database.BeginTransaction();
            var guildModel = new GuildEntity(name, leaderId);
            dbContext.Guilds.Add(guildModel);
            dbContext.SaveChanges();

            dbContext.Characters.Where(x => x.Id == leaderId).ExecuteUpdate(x => x.SetProperty(y => y.GuildId, guildModel.GuildId));
            transaction.Commit();
            return guildModel.GuildId;
        }

        public static IGuild? FindGuildFromDB(int guildId)
        {
            using var dbContext = new DBContext();
            var dbModel = dbContext.Guilds.FirstOrDefault(x => x.GuildId == guildId);
            if (dbModel == null)
                return null;

            var world = dbContext.Characters.Where(x => x.GuildId == guildId).Select(x => x.World).FirstOrDefault();
            var players = Server.getInstance().getWorld(world).getPlayerStorage();

            var members = dbContext.Characters.Where(x => x.GuildId == guildId).Select(x => x.Id).ToList();
            var memberList = players.GetPlayersByIds(members).OrderBy(x => x.GuildRank).ThenBy(x => x.Name).ToList();
            var guidModel = new Guild(world, memberList);
            var guild = GlobalTools.Mapper.Map<GuildEntity, Guild>(dbModel, guidModel);
            return guild;
        }

        public static GuildResponse? SendInvitation(IChannelClient c, string targetName)
        {
            var sender = c.OnlinedCharacter;

            var mc = c.CurrentServer.getPlayerStorage().getCharacterByName(targetName);
            if (mc == null)
            {
                return GuildResponse.NOT_IN_CHANNEL;
            }
            if (mc.GuildId > 0)
            {
                return GuildResponse.ALREADY_IN_GUILD;
            }

            if (InviteType.GUILD.CreateInvite(new GuildInviteRequest(sender, mc)))
            {
                mc.sendPacket(GuildPackets.guildInvite(sender.GuildId, sender.getName()));
                return null;
            }
            else
            {
                return GuildResponse.MANAGING_INVITE;
            }
        }

        public static bool AnswerInvitation(IPlayer answer, int guildId, bool operation)
        {
            InviteResult res = InviteType.GUILD.AnswerInvite(answer.Id, guildId, operation);

            GuildResponse? mgr = null;
            switch (res.Result)
            {
                case InviteResultType.ACCEPTED:
                    return true;

                case InviteResultType.DENIED:
                    mgr = GuildResponse.DENIED_INVITE;
                    break;

                default:
                    mgr = GuildResponse.NOT_FOUND_INVITE;
                    break;
            }

            if (mgr != null && res.Request != null)
            {
                res.Request.From.sendPacket(mgr.Value.getPacket(answer.getName()));
            }
            return false;
        }

        public static HashSet<IPlayer> getEligiblePlayersForGuild(IPlayer guildLeader)
        {
            HashSet<IPlayer> guildMembers = new();
            guildMembers.Add(guildLeader);

            MatchCheckerCoordinator mmce = guildLeader.getWorldServer().getMatchCheckerCoordinator();
            foreach (var chr in guildLeader.getMap().getAllPlayers())
            {
                if (chr.getParty() == null && chr.getGuild() == null && mmce.getMatchConfirmationLeaderid(chr.getId()) == -1)
                {
                    guildMembers.Add(chr);
                }
            }

            return guildMembers;
        }

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
