/*
    This file is part of the HeavenMS MapleStory Server
    Copyleft (L) 2016 - 2019 RonanLana

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation version 3 as published by
    the Free Software Foundation. You may not use, modify or distribute
    this program under any other version of the GNU Affero General Public
    License.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/


using Microsoft.EntityFrameworkCore;

namespace server.expeditions;

/**
 * @author Conrad
 * @author Ronan
 */
public class ExpeditionBossLog
{

    public class BossLogEntry : EnumClass
    {
        public static readonly BossLogEntry ZAKUM = new BossLogEntry(2, 1, false);
        public static readonly BossLogEntry HORNTAIL = new BossLogEntry(2, 1, false);
        public static readonly BossLogEntry PINKBEAN = new BossLogEntry(1, 1, false);
        public static readonly BossLogEntry SCARGA = new BossLogEntry(1, 1, false);
        public static readonly BossLogEntry PAPULATUS = new BossLogEntry(2, 1, false);

        private int entries;
        private int timeLength;
        private int minChannel;
        private int maxChannel;
        private bool week;

        public int MinChannel { get => minChannel; set => minChannel = value; }
        public int MaxChannel { get => maxChannel; set => maxChannel = value; }
        public int Entries { get => entries; set => entries = value; }
        public bool Week { get => week; set => week = value; }

        BossLogEntry(int entries, int timeLength, bool week) : this(entries, 0, int.MaxValue, timeLength, week)
        {

        }

        BossLogEntry(int entries, int minChannel, int maxChannel, int timeLength, bool week)
        {
            this.entries = entries;
            this.minChannel = minChannel;
            this.maxChannel = maxChannel;
            this.timeLength = timeLength;
            this.week = week;
        }

        public static List<KeyValuePair<DateTimeOffset, BossLogEntry>> getBossLogResetTimestamps(DateTimeOffset timeNow, bool week)
        {
            List<KeyValuePair<DateTimeOffset, BossLogEntry>> resetTimestamps = new();

            foreach (BossLogEntry b in EnumClassUtils.GetValues<BossLogEntry>())
            {
                if (b.week == week)
                {
                    resetTimestamps.Add(new(timeNow, b));
                }
            }

            return resetTimestamps;
        }

        public static BossLogEntry getBossEntryByName(string name)
        {
            foreach (BossLogEntry b in EnumClassUtils.GetValues<BossLogEntry>())
            {
                if (name == b.name())
                {
                    return b;
                }
            }

            return null;
        }
    }

    public static void resetBossLogTable()
    {
        /*
        Boss logs resets 12am, weekly thursday 12AM - thanks Smitty Werbenjagermanjensen (superadlez) - https://www.reddit.com/r/Maplestory/comments/61tiup/about_reset_time/
        */
        var thursday = DateTimeOffset.UtcNow.Date;
        int delta = DayOfWeek.Thursday - thursday.DayOfWeek;
        //if (delta <= 0)
        //    delta += 7; // 如加7天到下一个周四
        thursday.AddDays(delta);

        DateTimeOffset now = DateTimeOffset.UtcNow;
        var deltaTime = now - thursday;    // 2x time: get Date into millis
        if (deltaTime.TotalHours < 12)
        {
            ExpeditionBossLog.resetBossLogTable(true, thursday);
        }


        ExpeditionBossLog.resetBossLogTable(false, now);
    }
    private static void resetBossLogTable(bool week, DateTimeOffset c)
    {
        List<KeyValuePair<DateTimeOffset, BossLogEntry>> resetTimestamps = BossLogEntry.getBossLogResetTimestamps(c, week);

        try
        {
            using var dbContext = new DBContext();

            foreach (var p in resetTimestamps)
            {
                if (week)
                    dbContext.BosslogWeeklies.Where(x => x.Attempttime <= p.Key && x.Bosstype == p.Value.name()).ExecuteDelete();
                else
                    dbContext.BosslogDailies.Where(x => x.Attempttime <= p.Key && x.Bosstype == p.Value.name()).ExecuteDelete();
            }
        }
        catch (Exception e)
        {
            Log.Logger.Error(e.ToString());
        }
    }

    private static string getBossLogTable(bool week)
    {
        return week ? "bosslog_weekly" : "bosslog_daily";
    }

    private static int countPlayerEntries(int cid, BossLogEntry boss)
    {
        int ret_count = -1;
        try
        {
            using var dbContext = new DBContext();
            ret_count = boss.Week
                ? dbContext.BosslogWeeklies.Count(x => x.CharacterId == cid && x.Bosstype == boss.name())
                : dbContext.BosslogDailies.Count(x => x.CharacterId == cid && x.Bosstype == boss.name());

            return ret_count;
        }
        catch (Exception e)
        {
            Log.Logger.Error(e.ToString());
            return -1;
        }
    }

    private static void insertPlayerEntry(int cid, BossLogEntry boss)
    {
        try
        {
            using var dbContext = new DBContext();

            if (boss.Week)
            {
                dbContext.Add(new BosslogWeekly(cid, boss.name()));
            }
            else
            {
                dbContext.Add(new BosslogDaily(cid, boss.name()));
            }
            dbContext.SaveChanges();
        }
        catch (Exception e)
        {
            Log.Logger.Error(e.ToString());
        }
    }

    public static bool attemptBoss(int cid, int channel, Expedition exped, bool log)
    {
        if (!YamlConfig.config.server.USE_ENABLE_DAILY_EXPEDITIONS)
        {
            return true;
        }

        BossLogEntry boss = BossLogEntry.getBossEntryByName(exped.getType().name());
        if (boss == null)
        {
            return true;
        }

        if (channel < boss.MinChannel || channel > boss.MaxChannel)
        {
            return false;
        }

        if (countPlayerEntries(cid, boss) >= boss.Entries)
        {
            return false;
        }

        if (log)
        {
            insertPlayerEntry(cid, boss);
        }
        return true;
    }
}
