using Application.Core.Game.Invites;
using Application.Core.Game.Relation;
using Application.Core.Channel;
using net.server.guild;
using System.Runtime.ConstrainedExecution;

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

        private static List<IPlayer> getPartyGuildMasters(WorldChannel server, Team party)
        {
            List<IPlayer> mcl = new();

            foreach (var chr in party.GetChannelMembers(server))
            {
                if (chr != null)
                {
                    var lchr = party.GetChannelLeader(server);
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



    }
}
