/*
	This file is part of the OdinMS Maple Story Server
    Copyright (C) 2008 Patrick Huy <patrick.huy@frz.cc>
		       Matthias Butz <matze@odinms.de>
		       Jan Christian Meyer <vimes@odinms.de>

    Copyleft (L) 2016 - 2019 RonanLana (HeavenMS)

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


using Application.Core.Client;
using Application.Core.Managers;
using Application.Core.model;
using client.inventory;
using client.inventory.manipulator;
using Microsoft.EntityFrameworkCore;
using net.server;
using server;
using service;
using tools;

namespace client.processor.npc;

/**
 * @author RonanLana - synchronization of Fredrick modules and operation results
 */
public class FredrickProcessor
{
    private static ILogger log = LogFactory.GetLogger(LogType.Fredrick);
    private static int[] dailyReminders = new int[] { 2, 5, 10, 15, 30, 60, 90, int.MaxValue };

    private NoteService noteService;

    public FredrickProcessor(NoteService noteService)
    {
        this.noteService = noteService;
    }

    private static byte canRetrieveFromFredrick(IPlayer chr, List<ItemInventoryType> items)
    {
        if (!Inventory.checkSpotsAndOwnership(chr, items))
        {
            List<int> itemids = new();
            foreach (var it in items)
            {
                itemids.Add(it.Item.getItemId());
            }

            if (chr.canHoldUniques(itemids))
            {
                return 0x22;
            }
            else
            {
                return 0x20;
            }
        }

        int netMeso = chr.getMerchantNetMeso();
        if (netMeso > 0)
        {
            if (!chr.canHoldMeso(netMeso))
            {
                return 0x1F;
            }
        }
        else
        {
            if (chr.getMeso() < -1 * netMeso)
            {
                return 0x21;
            }
        }

        return 0x0;
    }

    private static string fredrickReminderMessage(int daynotes)
    {
        string msg;

        if (daynotes < 4)
        {
            msg = "Hi customer! I am Fredrick, the Union Chief of the Hired Merchant Union. A reminder that " + dailyReminders[daynotes] + " days have passed since you used our service. Please reclaim your stored goods at FM Entrance.";
        }
        else
        {
            msg = "Hi customer! I am Fredrick, the Union Chief of the Hired Merchant Union. " + dailyReminders[daynotes] + " days have passed since you used our service. Consider claiming back the items before we move them away for refund.";
        }

        return msg;
    }

    public static void removeFredrickLog(DBContext dbContext, int cid)
    {
        try
        {
            dbContext.Fredstorages.Where(x => x.Cid == cid).ExecuteDelete();
        }
        catch (Exception sqle)
        {
            log.Error(sqle.ToString());
        }
    }
    public static void insertFredrickLog(DBContext dbContext, int cid)
    {
        try
        {
            var dbModel = new Fredstorage()
            {
                Cid = cid,
                Daynotes = 0,
                Timestamp = DateTimeOffset.UtcNow
            };

            dbContext.Fredstorages.Where(x => x.Cid == cid).ExecuteDelete();
            dbContext.Fredstorages.Add(dbModel);
            dbContext.SaveChanges();
        }
        catch (Exception sqle)
        {
            log.Error(sqle.ToString());
        }
    }

    private static void removeFredrickReminders(DBContext dbContext, List<CharacterIdWorldPair> expiredCids)
    {
        List<string> expiredCnames = expiredCids.Select(x => x.Name).ToList();
        try
        {
            dbContext.Notes.Where(x =>  x.From == "FREDRICK" && expiredCnames.Contains(x.To)).ExecuteDelete();
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
        }
    }

    public void runFredrickSchedule()
    {
        try
        {
            using var dbContext = new DBContext();
            using var dbTrans = dbContext.Database.BeginTransaction();
            List<CharacterIdWorldPair> expiredCids = new();
            List<KeyValuePair<CharacterIdNamePair, int>> notifCids = new();
            var dataList = (from a in dbContext.Fredstorages
                            join b in dbContext.Characters on a.Cid equals b.Id
                            select new { data = a, CId = b.Id, b.Name, b.World, b.LastLogoutTime }).ToList();

            dataList.ForEach(x =>
            {
                int daynotes = Math.Min(dailyReminders.Length - 1, x.data.Daynotes);

                int elapsedDays = TimeUtils.DayDiff(x.data.Timestamp, DateTimeOffset.UtcNow);
                if (elapsedDays > 100)
                {
                    expiredCids.Add(new(x.CId, x.Name, x.World));
                }
                else
                {
                    int notifDay = dailyReminders[daynotes];

                    if (elapsedDays >= notifDay)
                    {
                        do
                        {
                            daynotes++;
                            notifDay = dailyReminders[daynotes];
                        } while (elapsedDays >= notifDay);

                        int inactivityDays = TimeUtils.DayDiff(x.LastLogoutTime, DateTimeOffset.UtcNow);

                        if (inactivityDays < 7 || daynotes >= dailyReminders.Length - 1)
                        {  // don't spam inactive players
                            string name = x.Name;
                            notifCids.Add(new(new(x.CId, name), daynotes));
                        }
                    }
                }
            });


            if (expiredCids.Count > 0)
            {
                var cidList = expiredCids.Select(x => x.CharacterId).ToList();
                var itemType = ItemFactory.MERCHANT.getValue();
                dbContext.Inventoryitems.Where(x => x.Type == itemType && cidList.Contains(x.Characterid ?? 0)).ExecuteDelete();

                foreach (var cid in expiredCids)
                {
                    var wserv = Server.getInstance().getWorld(cid.World);
                    if (wserv != null)
                    {
                        var chr = wserv.getPlayerStorage().getCharacterById(cid.CharacterId);
                        if (chr != null && chr.IsOnlined)
                        {
                            chr.setMerchantMeso(0);
                        }
                    }
                }
                dbContext.Characters.Where(x => cidList.Contains(x.Id)).ExecuteUpdate(x => x.SetProperty(y => y.MerchantMesos, 0));


                removeFredrickReminders(dbContext, expiredCids);

                dbContext.Fredstorages.Where(x => cidList.Contains(x.Cid)).ExecuteDelete();
            }

            if (notifCids.Count > 0)
            {

                foreach (var cid in notifCids)
                {
                    dbContext.Fredstorages.Where(x => x.Cid == cid.Key.Id).ExecuteUpdate(x => x.SetProperty(y => y.Daynotes, cid.Value));


                    string msg = fredrickReminderMessage(cid.Value - 1);
                    noteService.sendNormal(msg, "FREDRICK", cid.Key.Name);
                }

            }
            dbTrans.Commit();
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
        }
    }

    private static bool deleteFredrickItems(DBContext dbContext, int cid)
    {
        try
        {
            var typeValue = ItemFactory.MERCHANT.getValue();
            dbContext.Inventoryitems.Where(x => x.Type == typeValue && x.Characterid == cid).ExecuteDelete();
            return true;
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
            return false;
        }
    }

    public void fredrickRetrieveItems(IChannelClient c)
    {     // thanks Gustav for pointing out the dupe on Fredrick handling
        if (c.tryacquireClient())
        {
            try
            {
                var chr = c.OnlinedCharacter;

                List<ItemInventoryType> items;
                try
                {
                    items = ItemFactory.MERCHANT.loadItems(chr.getId(), false);

                    byte response = canRetrieveFromFredrick(chr, items);
                    if (response != 0)
                    {
                        chr.sendPacket(PacketCreator.fredrickMessage(response));
                        return;
                    }

                    chr.withdrawMerchantMesos();

                    using var dbContext = new DBContext();
                    if (deleteFredrickItems(dbContext, chr.getId()))
                    {
                        var merchant = chr.getHiredMerchant();

                        if (merchant != null)
                        {
                            merchant.clearItems();
                        }

                        foreach (var it in items)
                        {
                            Item item = it.Item;
                            InventoryManipulator.addFromDrop(chr.Client, item, false);
                            var itemName = ItemInformationProvider.getInstance().getName(item.getItemId());
                            log.Debug("Chr {CharacterName} gained {ItemQuantity}x {ItemName} ({CharacterId})", chr.getName(), item.getQuantity(), itemName, item.getItemId());
                        }

                        chr.sendPacket(PacketCreator.fredrickMessage(0x1E));
                        removeFredrickLog(dbContext, chr.getId());
                    }
                    else
                    {
                        chr.message("An unknown error has occured.");
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex.ToString());
                }
            }
            finally
            {
                c.releaseClient();
            }
        }
    }
}
