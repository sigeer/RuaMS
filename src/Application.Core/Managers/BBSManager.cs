using Microsoft.EntityFrameworkCore;
using net.server;
using net.server.guild;

namespace Application.Core.Managers
{
    public class BBSManager
    {
        readonly static ILogger log = LogFactory.GetLogger(LogType.BBS);
        public static void listBBSThreads(IChannelClient c, int start)
        {
            try
            {
                using var dbContext = new DBContext();
                var dataList = dbContext.BbsThreads.Where(x => x.Guildid == c.OnlinedCharacter.GuildId).OrderByDescending(x => x.Localthreadid).ToList();
                c.sendPacket(GuildPackets.BBSThreadList(dataList, start));

            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }
        }

        public static void newBBSReply(IChannelClient c, int localthreadid, string text)
        {
            if (c.OnlinedCharacter.GuildId <= 0)
            {
                return;
            }
            try
            {
                using var dbContext = new DBContext();
                using var dbTrans = dbContext.Database.BeginTransaction();
                var dbModel = dbContext.BbsThreads.Where(x => x.Guildid == c.OnlinedCharacter.GuildId && x.Localthreadid == localthreadid).Select(x => new { x.Threadid }).FirstOrDefault();
                if (dbModel == null)
                {
                    return;
                }
                int threadid = dbModel.Threadid;


                var newModel = new BbsReply(threadid, c.OnlinedCharacter.Id, Server.getInstance().getCurrentTime(), text);
                dbContext.BbsReplies.Add(newModel);
                dbContext.SaveChanges();

                dbContext.BbsThreads.Where(x => x.Threadid == threadid).ExecuteUpdate(x => x.SetProperty(y => y.Replycount, y => y.Replycount + 1));

                displayThread(dbContext, c, localthreadid);
                dbTrans.Commit();
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }
        }

        public static void editBBSThread(IChannelClient client, string title, string text, int icon, int localthreadid)
        {
            var chr = client.OnlinedCharacter;
            if (chr.getGuildId() < 1)
            {
                return;
            }
            try
            {

                using var dbContext = new DBContext();
                using var dbTrans = dbContext.Database.BeginTransaction();
                dbContext.BbsThreads.Where(x => x.Guildid == chr.GuildId && x.Localthreadid == localthreadid && x.Postercid == chr.Id || chr.GuildRank < 3)
                    .ExecuteUpdate(x => x.SetProperty(y => y.Name, title).SetProperty(y => y.Timestamp, Server.getInstance().getCurrentTime()).SetProperty(y => y.Icon, icon).SetProperty(y => y.Startpost, text));

                displayThread(dbContext, client, localthreadid);
                dbTrans.Commit();
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }
        }

        public static void newBBSThread(IChannelClient client, string title, string text, int icon, bool bNotice)
        {
            var chr = client.OnlinedCharacter;
            if (chr.GuildId <= 0)
            {
                return;
            }
            try
            {
                if (!bNotice)
                {
                    using var dbContext = new DBContext();
                    using var dbTrans = dbContext.Database.BeginTransaction();
                    var maxLocalThreadId = dbContext.BbsThreads.Where(x => x.Guildid == chr.GuildId).Max(x => x.Localthreadid) + 1;

                    var newModel = new BbsThread()
                    {
                        Postercid = chr.Id,
                        Timestamp = Server.getInstance().getCurrentTime(),
                        Name = title,
                        Icon = (short)icon,
                        Startpost = text,
                        Guildid = chr.GuildId,
                        Localthreadid = maxLocalThreadId
                    };
                    dbContext.BbsThreads.Add(newModel);
                    dbContext.SaveChanges();

                    displayThread(dbContext, client, maxLocalThreadId);
                    dbTrans.Commit();
                }
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }

        }

        public static void deleteBBSThread(IChannelClient client, int localthreadid)
        {
            var mc = client.OnlinedCharacter;
            if (mc.getGuildId() <= 0)
            {
                return;
            }

            try
            {

                using var dbContext = new DBContext();
                var filteredData = dbContext.BbsThreads.Where(x => x.Guildid == mc.GuildId && x.Localthreadid == localthreadid)
                    .Select(x => new { x.Threadid, x.Postercid })
                    .FirstOrDefault();

                if (filteredData == null)
                {
                    return;
                }
                if (mc.Id != filteredData.Postercid && mc.GuildRank > 2)
                {
                    return;
                }

                using var dbTrans = dbContext.Database.BeginTransaction();
                dbContext.BbsReplies.Where(x => x.Threadid == filteredData.Threadid).ExecuteDelete();
                dbContext.BbsThreads.Where(x => x.Threadid == filteredData.Threadid).ExecuteDelete();
                dbTrans.Commit();
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }
        }

        public static void deleteBBSReply(IChannelClient client, int replyid)
        {
            var mc = client.OnlinedCharacter;
            if (mc.getGuildId() <= 0)
            {
                return;
            }

            int threadid;
            try
            {

                using var dbContext = new DBContext();
                var dbModel = dbContext.BbsReplies.Where(x => x.Replyid == replyid).Select(x => new { x.Postercid, x.Threadid }).FirstOrDefault();
                if (dbModel == null)
                {
                    return;
                }
                if (mc.Id != dbModel.Postercid && mc.GuildRank > 2)
                {
                    return;
                }
                threadid = dbModel.Threadid;

                using var dbTrans = dbContext.Database.BeginTransaction();
                dbContext.BbsReplies.Where(x => x.Replyid == replyid).ExecuteDelete();
                dbContext.BbsThreads.Where(x => x.Threadid == threadid).ExecuteUpdate(x => x.SetProperty(y => y.Replycount, y => y.Replycount - 1));

                displayThread(dbContext, client, threadid, false);
                dbTrans.Commit();
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }
        }

        public static void displayThread(DBContext dbContext, IChannelClient client, int threadid, bool bIsThreadIdLocal = true)
        {
            var mc = client.OnlinedCharacter;
            if (mc.GuildId <= 0)
            {
                return;
            }

            try
            {
                var dbModel = dbContext.BbsThreads.Where(x => x.Guildid == mc.GuildId && (bIsThreadIdLocal ? x.Localthreadid == threadid : x.Threadid == threadid)).FirstOrDefault();
                if (dbModel == null)
                {
                    return;
                }

                if (dbModel.Replycount >= 0)
                {
                    var thid = !bIsThreadIdLocal ? threadid : dbModel.Threadid;
                    var replies = dbContext.BbsReplies.Where(x => x.Threadid == thid).ToList();
                    client.sendPacket(GuildPackets.showThread(bIsThreadIdLocal ? threadid : dbModel.Localthreadid, dbModel, replies));
                }

            }
            catch (Exception e)
            {
                log.Error(e, "Error displaying thread");
            }
        }
    }
}
