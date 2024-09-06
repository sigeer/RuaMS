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


using Application.Core.model;
using client.autoban;
using client.inventory;
using client.inventory.manipulator;
using constants.id;
using constants.inventory;
using Microsoft.EntityFrameworkCore;
using server;
using tools;

namespace client.processor.npc;


/**
 * @author RonanLana - synchronization of Duey modules
 */
public class DueyProcessor
{
    private static ILogger log = LogFactory.GetLogger("DueyProcessor");

    private static CharacterAccountIdPair getAccountCharacterIdFromCNAME(string name)
    {
        try
        {
            using var dbContext = new DBContext();

            return dbContext.Characters.Where(x => x.Name == name).Select(x => new CharacterAccountIdPair(x.AccountId, x.Id)).FirstOrDefault() ?? new CharacterAccountIdPair(-1, -1);
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
            return new CharacterAccountIdPair(-1, -1);
        }
    }

    private static void showDueyNotification(IClient c, IPlayer player)
    {
        try
        {
            using var dbContext = new DBContext();
            var sender = dbContext.Dueypackages.Where(x => x.ReceiverId == player.getId() && x.Checked).OrderByDescending(x => x.Type).Select(x => new { x.SenderName, x.Type }).FirstOrDefault();
            if (sender != null)
            {
                dbContext.Dueypackages.Where(x => x.ReceiverId == player.getId()).ExecuteUpdate(x => x.SetProperty(y => y.Checked, false));
                c.sendPacket(PacketCreator.sendDueyParcelReceived(sender.SenderName, sender.Type));
            }

        }
        catch (Exception e)
        {
            log.Error(e.ToString());
        }
    }

    private static void deletePackageFromInventoryDB(DBContext dbContext, int packageId)
    {
        ItemFactory.DUEY.saveItems([], packageId, dbContext);
    }

    private static void removePackageFromDB(int packageId)
    {
        try
        {
            using var dbContext = new DBContext();
            using var dbTrans = dbContext.Database.BeginTransaction();
            dbContext.Dueypackages.Where(x => x.PackageId == packageId).ExecuteDelete();

            deletePackageFromInventoryDB(dbContext, packageId);
            dbTrans.Commit();
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
        }
    }

    private static Dueypackage getPackageFromDB(Dueypackage rs)
    {
        try
        {
            var dueyItems = ItemFactory.DUEY.loadItems(rs.PackageId, false);

            if (dueyItems.Count > 0)
            {
                rs.Item = dueyItems.get(0).Item;
            }
            rs.UpdateSentTime();
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
        }
        return rs;
    }

    private static List<Dueypackage> loadPackages(IPlayer chr)
    {
        List<Dueypackage> packages = new();
        try
        {
            using var dbContext = new DBContext();
            var dataList = dbContext.Dueypackages.Where(x => x.ReceiverId == chr.getId()).ToList();
            dataList.ForEach(x =>
            {
                packages.Add(getPackageFromDB(x));
            });

        }
        catch (Exception e)
        {
            log.Error(e.ToString());
        }

        return packages;
    }

    private static int createPackage(int mesos, string? message, string sender, int toCid, bool quick)
    {
        try
        {
            using var dbContext = new DBContext();
            var dbModel = new Dueypackage(toCid, sender, mesos, message, true, quick);
            dbContext.Dueypackages.Add(dbModel);


            if (dbContext.SaveChanges() < 1)
            {
                log.Error("Error trying to create namespace [mesos: {Meso}, sender: {Sender}, quick: {Quick}, receiver chrId: {Receiver}]", mesos, sender, quick, toCid);
                return -1;
            }


            return dbModel.PackageId;
        }
        catch (Exception sqle)
        {
            log.Error(sqle.ToString());
        }

        return -1;
    }

    private static bool insertPackageItem(int packageId, Item item)
    {
        try
        {
            ItemInventoryType dueyItem = new(item, InventoryTypeUtils.getByType(item.getItemType()));
            using var dbContext = new DBContext();
            using var dbTrans = dbContext.Database.BeginTransaction();
            ItemFactory.DUEY.saveItems(Collections.singletonList(dueyItem), packageId, dbContext);
            dbTrans.Commit();
            return true;
        }
        catch (Exception sqle)
        {
            log.Error(sqle.ToString());
        }

        return false;
    }

    private static int addPackageItemFromInventory(int packageId, IClient c, sbyte invTypeId, short itemPos, short amount)
    {
        if (invTypeId > 0)
        {
            ItemInformationProvider ii = ItemInformationProvider.getInstance();

            InventoryType invType = InventoryTypeUtils.getByType(invTypeId);
            Inventory inv = c.OnlinedCharacter.getInventory(invType);

            Item? item;
            inv.lockInventory();
            try
            {
                item = inv.getItem(itemPos);
                if (item != null && item.getQuantity() >= amount)
                {
                    if (item.isUntradeable() || ii.isUnmerchable(item.getItemId()))
                    {
                        return -1;
                    }

                    if (ItemConstants.isRechargeable(item.getItemId()))
                    {
                        InventoryManipulator.removeFromSlot(c, invType, itemPos, item.getQuantity(), true);
                    }
                    else
                    {
                        InventoryManipulator.removeFromSlot(c, invType, itemPos, amount, true, false);
                    }

                    item = item.copy();
                }
                else
                {
                    return -2;
                }
            }
            finally
            {
                inv.unlockInventory();
            }

            KarmaManipulator.toggleKarmaFlagToUntradeable(item);
            item.setQuantity(amount);

            if (!insertPackageItem(packageId, item))
            {
                return 1;
            }
        }

        return 0;
    }

    public static void dueySendItem(IClient c, sbyte invTypeId, short itemPos, short amount, int sendMesos, string? sendMessage, string recipient, bool quick)
    {
        if (c.tryacquireClient())
        {
            try
            {
                if (c.OnlinedCharacter.isGM() && c.OnlinedCharacter.gmLevel() < YamlConfig.config.server.MINIMUM_GM_LEVEL_TO_USE_DUEY)
                {
                    c.OnlinedCharacter.message("You cannot use Duey to send items at your GM level.");
                    log.Information("GM {GM} tried to send a namespace to {Recipient}", c.OnlinedCharacter.getName(), recipient);
                    c.sendPacket(PacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_SEND_INCORRECT_REQUEST.getCode()));
                    return;
                }

                int fee = Trade.getFee(sendMesos);
                if (sendMessage != null && sendMessage.Length > 100)
                {
                    AutobanFactory.PACKET_EDIT.alert(c.OnlinedCharacter, c.OnlinedCharacter.getName() + " tried to packet edit with Quick Delivery on duey.");
                    log.Warning("Chr {CharacterName} tried to use duey with too long of a text", c.OnlinedCharacter.getName());
                    c.disconnect(true, false);
                    return;
                }
                if (!quick)
                {
                    fee += 5000;
                }
                else if (!c.OnlinedCharacter.haveItem(ItemId.QUICK_DELIVERY_TICKET))
                {
                    AutobanFactory.PACKET_EDIT.alert(c.OnlinedCharacter, c.OnlinedCharacter.getName() + " tried to packet edit with Quick Delivery on duey.");
                    log.Warning("Chr {CharacterName} tried to use duey with Quick Delivery without a ticket, mesos {Meso} and amount {Amount}", c.OnlinedCharacter.getName(), sendMesos, amount);
                    c.disconnect(true, false);
                    return;
                }

                long finalcost = (long)sendMesos + fee;
                if (finalcost < 0 || finalcost > int.MaxValue || (amount < 1 && sendMesos == 0))
                {
                    AutobanFactory.PACKET_EDIT.alert(c.OnlinedCharacter, c.OnlinedCharacter.getName() + " tried to packet edit with duey.");
                    log.Warning("Chr {CharacterName} tried to use duey with mesos {Meso} and amount {Amount}", c.OnlinedCharacter.getName(), sendMesos, amount);
                    c.disconnect(true, false);
                    return;
                }

                if (c.OnlinedCharacter.getMeso() < finalcost)
                {
                    c.sendPacket(PacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_SEND_NOT_ENOUGH_MESOS.getCode()));
                    return;
                }

                var accIdCid = getAccountCharacterIdFromCNAME(recipient);
                var recipientAccId = accIdCid.AccountId;
                var recipientCid = accIdCid.CharacterId;

                if (recipientAccId == -1 || recipientCid == -1)
                {
                    c.sendPacket(PacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_SEND_NAME_DOES_NOT_EXIST.getCode()));
                    return;
                }

                if (recipientAccId == c.getAccID())
                {
                    c.sendPacket(PacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_SEND_SAMEACC_ERROR.getCode()));
                    return;
                }

                if (quick)
                {
                    InventoryManipulator.removeById(c, InventoryType.CASH, ItemId.QUICK_DELIVERY_TICKET, 1, false, false);
                }

                int packageId = createPackage(sendMesos, sendMessage, c.OnlinedCharacter.getName(), recipientCid, quick);
                if (packageId == -1)
                {
                    c.sendPacket(PacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_SEND_ENABLE_ACTIONS.getCode()));
                    return;
                }
                c.OnlinedCharacter.gainMeso((int)-finalcost, false);

                int res = addPackageItemFromInventory(packageId, c, invTypeId, itemPos, amount);
                if (res == 0)
                {
                    c.sendPacket(PacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_SEND_SUCCESSFULLY_SENT.getCode()));
                }
                else if (res > 0)
                {
                    c.sendPacket(PacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_SEND_ENABLE_ACTIONS.getCode()));
                }
                else
                {
                    c.sendPacket(PacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_SEND_INCORRECT_REQUEST.getCode()));
                }

                IClient? rClient = null;
                int channel = c.getWorldServer().find(recipient);
                if (channel > -1)
                {
                    var rcserv = c.getWorldServer().getChannel(channel);
                    if (rcserv != null)
                    {
                        var rChr = rcserv.getPlayerStorage().getCharacterByName(recipient);
                        if (rChr != null)
                        {
                            rClient = rChr.getClient();
                        }
                    }
                }

                if (rClient != null && rClient.isLoggedIn() && !rClient.OnlinedCharacter.isAwayFromWorld())
                {
                    showDueyNotification(rClient, rClient.OnlinedCharacter);
                }
            }
            finally
            {
                c.releaseClient();
            }
        }
    }

    public static void dueyRemovePackage(IClient c, int packageid, bool playerRemove)
    {
        if (c.tryacquireClient())
        {
            try
            {
                removePackageFromDB(packageid);
                c.sendPacket(PacketCreator.removeItemFromDuey(playerRemove, packageid));
            }
            finally
            {
                c.releaseClient();
            }
        }
    }

    public static void dueyClaimPackage(IClient c, int packageId)
    {
        if (c.tryacquireClient())
        {
            try
            {
                try
                {
                    Dueypackage? dp = null;
                    using var dbContext = new DBContext();
                    var dataItem = dbContext.Dueypackages.Where(x => x.PackageId == packageId).FirstOrDefault();
                    if (dataItem != null)
                    {
                        dp = getPackageFromDB(dataItem);
                    }


                    if (dp == null)
                    {
                        c.sendPacket(PacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_RECV_UNKNOWN_ERROR.getCode()));
                        log.Warning("Chr {CharacterName} tried to receive namespace from duey with id {PackageId}", c.OnlinedCharacter.getName(), packageId);
                        return;
                    }

                    if (dp.isDeliveringTime())
                    {
                        c.sendPacket(PacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_RECV_UNKNOWN_ERROR.getCode()));
                        return;
                    }

                    var dpItem = dp.Item;
                    if (dpItem != null)
                    {
                        if (!c.OnlinedCharacter.canHoldMeso(dp.Mesos))
                        {
                            c.sendPacket(PacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_RECV_UNKNOWN_ERROR.getCode()));
                            return;
                        }

                        if (!InventoryManipulator.checkSpace(c, dpItem.getItemId(), dpItem.getQuantity(), dpItem.getOwner()))
                        {
                            int itemid = dpItem.getItemId();
                            if (ItemInformationProvider.getInstance().isPickupRestricted(itemid) && c.OnlinedCharacter.getInventory(ItemConstants.getInventoryType(itemid)).findById(itemid) != null)
                            {
                                c.sendPacket(PacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_RECV_RECEIVER_WITH_UNIQUE.getCode()));
                            }
                            else
                            {
                                c.sendPacket(PacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_RECV_NO_FREE_SLOTS.getCode()));
                            }

                            return;
                        }
                        else
                        {
                            InventoryManipulator.addFromDrop(c, dpItem, false);
                        }
                    }

                    c.OnlinedCharacter.gainMeso(dp.Mesos, false);

                    dueyRemovePackage(c, packageId, false);
                }
                catch (Exception e)
                {
                    log.Error(e.ToString());
                }
            }
            finally
            {
                c.releaseClient();
            }
        }
    }

    public static void dueySendTalk(IClient c, bool quickDelivery)
    {
        if (c.tryacquireClient())
        {
            try
            {
                long timeNow = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                if (timeNow - c.OnlinedCharacter.getNpcCooldown() < YamlConfig.config.server.BLOCK_NPC_RACE_CONDT)
                {
                    c.sendPacket(PacketCreator.enableActions());
                    return;
                }
                c.OnlinedCharacter.setNpcCooldown(timeNow);

                if (quickDelivery)
                {
                    c.sendPacket(PacketCreator.sendDuey(0x1A, []));
                }
                else
                {
                    c.sendPacket(PacketCreator.sendDuey(0x8, loadPackages(c.OnlinedCharacter)));
                }
            }
            finally
            {
                c.releaseClient();
            }
        }
    }

    public static void dueyCreatePackage(Item item, int mesos, string sender, int recipientCid)
    {
        int packageId = createPackage(mesos, null, sender, recipientCid, false);
        if (packageId != -1)
        {
            insertPackageItem(packageId, item);
        }
    }

    public static void runDueyExpireSchedule()
    {

        try
        {
            var dayBefore30 = DateTimeOffset.Now.AddDays(-30);
            using var dbContext = new DBContext();
            var toRemove = dbContext.Dueypackages.Where(x => x.TimeStamp < dayBefore30).Select(X => X.PackageId);


            foreach (int pid in toRemove)
            {
                removePackageFromDB(pid);
            }

            dbContext.Dueypackages.Where(x => x.TimeStamp < dayBefore30).ExecuteDelete();
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
        }
    }
}

public enum DueyProcessorActions
{
    TOSERVER_RECV_ITEM = 0x00,
    TOSERVER_SEND_ITEM = 0x02,
    TOSERVER_CLAIM_PACKAGE = 0x04,
    TOSERVER_REMOVE_PACKAGE = 0x05,
    TOSERVER_CLOSE_DUEY = 0x07,
    TOCLIENT_OPEN_DUEY = 0x08,
    TOCLIENT_SEND_ENABLE_ACTIONS = 0x09,
    TOCLIENT_SEND_NOT_ENOUGH_MESOS = 0x0A,
    TOCLIENT_SEND_INCORRECT_REQUEST = 0x0B,
    TOCLIENT_SEND_NAME_DOES_NOT_EXIST = 0x0C,
    TOCLIENT_SEND_SAMEACC_ERROR = 0x0D,
    TOCLIENT_SEND_RECEIVER_STORAGE_FULL = 0x0E,
    TOCLIENT_SEND_RECEIVER_UNABLE_TO_RECV = 0x0F,
    TOCLIENT_SEND_RECEIVER_STORAGE_WITH_UNIQUE = 0x10,
    TOCLIENT_SEND_MESO_LIMIT = 0x11,
    TOCLIENT_SEND_SUCCESSFULLY_SENT = 0x12,
    TOCLIENT_RECV_UNKNOWN_ERROR = 0x13,
    TOCLIENT_RECV_ENABLE_ACTIONS = 0x14,
    TOCLIENT_RECV_NO_FREE_SLOTS = 0x15,
    TOCLIENT_RECV_RECEIVER_WITH_UNIQUE = 0x16,
    TOCLIENT_RECV_SUCCESSFUL_MSG = 0x17,
    TOCLIENT_RECV_PACKAGE_MSG = 0x1B
}

public static class ActionsExntesions
{
    public static byte getCode(this DueyProcessorActions a)
    {
        return (byte)a;
    }
}