/*
 This file is part of the OdinMS Maple Story Server
 Copyright (C) 2008 Patrick Huy <patrick.huy@frz.cc>
 Matthias Butz <matze@odinms.de>
 Jan Christian Meyer <vimes@odinms.de>

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


using client;
using Microsoft.EntityFrameworkCore;
using net.packet;
using net.server.guild;

namespace net.server.channel.handlers;

public class BBSOperationHandler : AbstractPacketHandler
{

    private string correctLength(string inValue, int maxSize)
    {
        return inValue.Length > maxSize ? inValue.Substring(0, maxSize) : inValue;
    }

    public override void handlePacket(InPacket p, Client c)
    {
        if (c.getPlayer().getGuildId() < 1)
        {
            return;
        }
        byte mode = p.readByte();
        int localthreadid = 0;
        switch (mode)
        {
            case 0:
                bool bEdit = p.readByte() == 1;
                if (bEdit)
                {
                    localthreadid = p.readInt();
                }
                bool bNotice = p.readByte() == 1;
                string title = correctLength(p.readString(), 25);
                string text = correctLength(p.readString(), 600);
                int icon = p.readInt();
                if (icon >= 0x64 && icon <= 0x6a)
                {
                    if (!c.getPlayer().haveItemWithId(5290000 + icon - 0x64, false))
                    {
                        return;
                    }
                }
                else if (icon < 0 || icon > 3)
                {
                    return;
                }
                if (!bEdit)
                {
                    newBBSThread(c, title, text, icon, bNotice);
                }
                else
                {
                    editBBSThread(c, title, text, icon, localthreadid);
                }
                break;
            case 1:
                localthreadid = p.readInt();
                deleteBBSThread(c, localthreadid);
                break;
            case 2:
                int start = p.readInt();
                listBBSThreads(c, start * 10);
                break;
            case 3: // list thread + reply, following by id (int)
                localthreadid = p.readInt();
                using (var dbContext = new DBContext())
                {
                    displayThread(dbContext, c, localthreadid);
                }
                break;
            case 4: // reply
                localthreadid = p.readInt();
                text = correctLength(p.readString(), 25);
                newBBSReply(c, localthreadid, text);
                break;
            case 5: // delete reply
                p.readInt(); // we don't use this
                int replyid = p.readInt();
                deleteBBSReply(c, replyid);
                break;
            default:
                //Console.WriteLine("Unhandled BBS mode: " + slea.ToString());
                break;
        }
    }

    private void listBBSThreads(Client c, int start)
    {
        try
        {

            using var dbContext = new DBContext();
            var dataList = dbContext.BbsThreads.Where(x => x.Guildid == c.getPlayer().getGuildId()).OrderByDescending(x => x.Localthreadid).ToList();
            c.sendPacket(GuildPackets.BBSThreadList(dataList, start));

        }
        catch (Exception e)
        {
            log.Error(e.ToString());
        }
    }

    private void newBBSReply(Client c, int localthreadid, string text)
    {
        if (c.getPlayer().getGuildId() <= 0)
        {
            return;
        }
        try
        {
            using var dbContext = new DBContext();
            using var dbTrans = dbContext.Database.BeginTransaction();
            var dbModel = dbContext.BbsThreads.Where(x => x.Guildid == c.getPlayer().getGuildId() && x.Localthreadid == localthreadid).Select(x => new { x.Threadid }).FirstOrDefault();
            if (dbModel == null)
            {
                return;
            }
            int threadid = dbModel.Threadid;


            var newModel = new BbsReply(threadid, c.getPlayer().getId(), currentServerTime(), text);
            dbContext.BbsReplies.Add(newModel);
            dbContext.SaveChanges();

            dbContext.BbsThreads.Where(x => x.Threadid == threadid).ExecuteUpdate(x => x.SetProperty(y => y.Replycount, y => y.Replycount + 1));

            displayThread(dbContext, c, localthreadid);
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
        }
    }

    private void editBBSThread(Client client, string title, string text, int icon, int localthreadid)
    {
        Character chr = client.getPlayer();
        if (chr.getGuildId() < 1)
        {
            return;
        }
        try
        {

            using var dbContext = new DBContext();
            using var dbTrans = dbContext.Database.BeginTransaction();
            dbContext.BbsThreads.Where(x => x.Guildid == chr.getGuildId() && x.Localthreadid == localthreadid && x.Postercid == chr.getId() || chr.getGuildRank() < 3)
                .ExecuteUpdate(x => x.SetProperty(y => y.Name, title).SetProperty(y => y.Timestamp, currentServerTime()).SetProperty(y => y.Icon, icon).SetProperty(y => y.Startpost, text));

            displayThread(dbContext, client, localthreadid);
            dbTrans.Commit();
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
        }
    }

    private void newBBSThread(Client client, string title, string text, int icon, bool bNotice)
    {
        Character chr = client.getPlayer();
        if (chr.getGuildId() <= 0)
        {
            return;
        }
        try
        {
            if (!bNotice)
            {
                using var dbContext = new DBContext();
                using var dbTrans = dbContext.Database.BeginTransaction();
                var maxLocalThreadId = dbContext.BbsThreads.Where(x => x.Guildid == chr.getGuildId()).Max(x => x.Localthreadid) + 1;

                var newModel = new BbsThread()
                {
                    Postercid = chr.getId(),
                    Timestamp = currentServerTime(),
                    Name = title,
                    Icon = (short)icon,
                    Startpost = text,
                    Guildid = chr.getGuildId(),
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

    public void deleteBBSThread(Client client, int localthreadid)
    {
        Character mc = client.getPlayer();
        if (mc.getGuildId() <= 0)
        {
            return;
        }

        try
        {

            using var dbContext = new DBContext();
            var filteredData = dbContext.BbsThreads.Where(x => x.Guildid == mc.getGuildId() && x.Localthreadid == localthreadid)
                .Select(x => new { x.Threadid, x.Postercid })
                .FirstOrDefault();

            if (filteredData == null)
            {
                return;
            }
            if (mc.getId() != filteredData.Postercid && mc.getGuildRank() > 2)
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

    public void deleteBBSReply(Client client, int replyid)
    {
        Character mc = client.getPlayer();
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
            if (mc.getId() != dbModel.Postercid && mc.getGuildRank() > 2)
            {
                return;
            }
            threadid = dbModel.Threadid;

            using var dbTrans = dbContext.Database.BeginTransaction();
            dbContext.BbsReplies.Where(x => x.Replyid == replyid).ExecuteDelete();
            dbContext.BbsThreads.Where(x => x.Threadid == threadid).ExecuteUpdate(x => x.SetProperty(y => y.Replycount, y => y.Replycount - 1));

            displayThread(dbContext, client, threadid, false);
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
        }
    }

    public void displayThread(DBContext dbContext, Client client, int threadid, bool bIsThreadIdLocal = true)
    {
        Character mc = client.getPlayer();
        if (mc.getGuildId() <= 0)
        {
            return;
        }

        try
        {
            var dbModel = dbContext.BbsThreads.Where(x => x.Guildid == mc.getGuildId() && (bIsThreadIdLocal ? x.Localthreadid == threadid : x.Threadid == threadid)).FirstOrDefault();
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
