using Application.Core.Game.Invites;
using Application.Core.Game.Relation;
using Application.Core.Game.TheWorld;
using Microsoft.EntityFrameworkCore;
using net.server;
using net.server.coordinator.world;
using net.server.guild;

namespace Application.Core.Managers
{
    public class AllianceManager
    {
        static readonly ILogger log = LogFactory.GetLogger(LogType.Alliance);
        public static bool canBeUsedAllianceName(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Contains(" ") || name.Length > 12)
            {
                return false;
            }

            using var dbContext = new DBContext();
            return dbContext.Alliances.Any(x => x.Name == name);
        }
        public static Alliance? createAllianceOnDb(List<int> guilds, string name)
        {
            // will create an alliance, where the first guild listed is the leader and the alliance name MUST BE already checked for unicity.

            try
            {
                using var dbContext = new DBContext();
                using var dbTrans = dbContext.Database.BeginTransaction();
                var newModel = new AllianceEntity(name);
                dbContext.Alliances.Add(newModel);
                dbContext.SaveChanges();
                dbContext.AllianceGuilds.AddRange(guilds.Select(x => new Allianceguild
                {
                    AllianceId = newModel.Id,
                    GuildId = x
                }));
                dbContext.SaveChanges();
                dbTrans.Commit();
                return new Alliance(newModel.Id, name);
            }
            catch (Exception ex)
            {
                log.Error(ex, "创建联盟失败");
                return null;
            }
        }

        private static List<IPlayer> getPartyGuildMasters(ITeam party)
        {
            List<IPlayer> mcl = new();

            foreach (var chr in party.getMembers())
            {
                if (chr != null)
                {
                    var lchr = party.getLeader();
                    if (chr.getGuildRank() == 1 && lchr != null && chr.getMapId() == lchr.getMapId())
                    {
                        mcl.Add(chr);
                    }
                }
            }

            if (mcl.Count > 0 && !mcl[0].isPartyLeader())
            {
                for (int i = 1; i < mcl.Count; i++)
                {
                    if (mcl[i].isPartyLeader())
                    {
                        (mcl[0], mcl[i]) = (mcl[i], mcl[0]);
                    }
                }
            }

            return mcl;
        }

        public static Alliance? createAlliance(ITeam party, string name)
        {
            var guildMasters = getPartyGuildMasters(party);
            if (guildMasters.Count != 2)
            {
                return null;
            }

            List<int> guilds = new();
            foreach (var mc in guildMasters)
            {
                guilds.Add(mc.getGuildId());
            }
            var alliance = AllianceManager.createAllianceOnDb(guilds, name);
            if (alliance != null)
            {
                alliance.setCapacity(guilds.Count);
                foreach (int g in guilds)
                {
                    alliance.AddGuild(g);
                }

                int id = alliance.getId();
                try
                {
                    for (int i = 0; i < guildMasters.Count; i++)
                    {
                        var guild = AllGuildStorage.GetGuildById(guilds[i]);
                        if (guild != null)
                        {
                            guild.setAllianceId(id);
                            guild.resetAllianceGuildPlayersRank();
                        }

                        var chr = guildMasters[i];
                        chr.AllianceRank = (i == 0) ? 1 : 2;
                        chr.saveGuildStatus();
                    }

                    AllAllianceStorage.AddOrUpdate(alliance);

                    int worldid = guildMasters.get(0).getWorld();
                    alliance.broadcastMessage(GuildPackets.updateAllianceInfo(alliance), -1, -1);
                    alliance.broadcastMessage(GuildPackets.getGuildAlliances(alliance), -1, -1);  // thanks Vcoc for noticing guilds from other alliances being visually stacked here due to this not being updated
                }
                catch (Exception e)
                {
                    log.Error(e.ToString());
                    return null;
                }
            }

            return alliance;
        }


        public static Alliance? loadAlliance(int id)
        {
            if (id <= 0)
            {
                return null;
            }
            Alliance alliance = new Alliance(-1, "");
            try
            {

                using var dbContext = new DBContext();
                var dbModel = dbContext.Alliances.Where(x => x.Id == id).FirstOrDefault();
                if (dbModel == null)
                    return null;

                GlobalTools.Mapper.Map(dbModel, alliance);

                var guilds = dbContext.AllianceGuilds.Where(x => x.AllianceId == dbModel.Id).Select(x => x.GuildId).ToList();
                guilds.ForEach(x =>
                {
                    alliance.AddGuild(x);
                });
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }

            return alliance;
        }
        public static void sendInvitation(IClient c, string targetGuildName, int allianceId)
        {
            var mg = AllGuildStorage.GetGuildByName(targetGuildName);
            if (mg == null)
            {
                c.OnlinedCharacter.dropMessage(5, "The entered guild does not exist.");
            }
            else
            {
                if (mg.getAllianceId() > 0)
                {
                    c.OnlinedCharacter.dropMessage(5, "The entered guild is already registered on a guild alliance.");
                }
                else
                {
                    var victim = mg.getMGC(mg.getLeaderId());
                    if (victim == null)
                    {
                        c.OnlinedCharacter.dropMessage(5, "The master of the guild that you offered an invitation is currently not online.");
                    }
                    else
                    {
                        if (InviteType.ALLIANCE.CreateInvite(new AllianceInviteRequest(c.OnlinedCharacter, victim)))
                        {
                            victim.sendPacket(GuildPackets.allianceInvite(allianceId, c.OnlinedCharacter));
                        }
                        else
                        {
                            c.OnlinedCharacter.dropMessage(5, "The master of the guild that you offered an invitation is currently managing another invite.");
                        }
                    }
                }
            }
        }

        public static bool answerInvitation(int targetId, string targetGuildName, int allianceId, bool answer)
        {
            InviteResult res = InviteType.ALLIANCE.AnswerInvite(targetId, allianceId, answer);

            string msg;
            switch (res.Result)
            {
                case InviteResultType.ACCEPTED:
                    return true;

                case InviteResultType.DENIED:
                    msg = "[" + targetGuildName + "] guild has denied your guild alliance invitation.";
                    break;

                default:
                    msg = "The guild alliance request has not been accepted, since the invitation expired.";
                    break;
            }

            if (res.Request != null)
            {
                res.Request.From.dropMessage(5, msg);
            }

            return false;
        }
    }
}
