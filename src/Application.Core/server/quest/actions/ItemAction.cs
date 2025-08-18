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


using Application.Core.Channel.DataProviders;
using client.inventory;
using client.inventory.manipulator;
using server.quest;
using tools;

namespace server.quest.actions;

/**
 * @author Tyler (Twdtwd)
 * @author Ronan
 */
public class ItemAction : AbstractQuestAction
{
    List<ItemData> items = new();

    public ItemAction(Quest quest, Data data) : base(QuestActionType.ITEM, quest)
    {

        processData(data);
    }


    public override void processData(Data data)
    {
        foreach (Data iEntry in data.getChildren())
        {
            int id = DataTool.getInt(iEntry.getChildByPath("id"));
            int count = DataTool.getInt(iEntry.getChildByPath("count"), 1);
            int period = DataTool.getInt(iEntry.getChildByPath("period"), 0);

            int? prop = null;
            var propData = iEntry.getChildByPath("prop");
            if (propData != null)
            {
                prop = DataTool.getInt(propData);
            }

            int gender = 2;
            if (iEntry.getChildByPath("gender") != null)
            {
                gender = DataTool.getInt(iEntry.getChildByPath("gender"));
            }

            int job = -1;
            if (iEntry.getChildByPath("job") != null)
            {
                job = DataTool.getInt(iEntry.getChildByPath("job"));
            }

            items.Add(new ItemData(int.Parse(iEntry.getName()), id, count, prop, job, gender, period));
        }

        items.Sort((o1, o2) => o1.map - o2.map);
    }

    public override void run(IPlayer chr, int? extSelection)
    {
        List<ItemData> takeItem = new();
        List<ItemData> giveItem = new();

        int props = 0, rndProps = 0, accProps = 0;
        foreach (ItemData item in items)
        {
            if (item.getProp() != null && item.getProp() != -1 && canGetItem(item, chr))
            {
                props += item.getProp()!.Value;
            }
        }

        int extNum = 0;
        if (props > 0)
        {
            rndProps = Randomizer.nextInt(props);
        }
        foreach (ItemData iEntry in items)
        {
            if (!canGetItem(iEntry, chr))
            {
                continue;
            }

            if (iEntry.getProp() != null)
            {
                if (iEntry.getProp() == -1)
                {
                    if (extSelection != extNum++)
                    {
                        continue;
                    }
                }
                else
                {
                    accProps += iEntry.getProp()!.Value;

                    if (accProps <= rndProps)
                    {
                        continue;
                    }
                    else
                    {
                        accProps = int.MinValue;
                    }
                }
            }

            if (iEntry.getCount() < 0)
            { // Remove Item
                takeItem.Add(iEntry);
            }
            else
            {                    // Give Item
                giveItem.Add(iEntry);
            }
        }

        // must take all needed items before giving others

        foreach (ItemData iEntry in takeItem)
        {
            int itemid = iEntry.getId(), count = iEntry.getCount();

            InventoryType type = ItemConstants.getInventoryType(itemid);
            int quantity = count * -1; // Invert
            if (type.Equals(InventoryType.EQUIP))
            {
                if (chr.getInventory(type).countById(itemid) < quantity)
                {
                    // Not enough in the equip inventoty, so check Equipped...
                    if (chr.getInventory(InventoryType.EQUIPPED).countById(itemid) > quantity)
                    {
                        // Found it equipped, so change the type to equipped.
                        type = InventoryType.EQUIPPED;
                    }
                }
            }

            InventoryManipulator.removeById(chr.Client, type, itemid, quantity, true, false);
            chr.sendPacket(PacketCreator.getShowItemGain(itemid, (short)count, true));
        }

        foreach (ItemData iEntry in giveItem)
        {
            int itemid = iEntry.getId(), count = iEntry.getCount(), period = iEntry.getPeriod();    // thanks Vcoc for noticing quest milestone item not getting removed from inventory after a while

            InventoryManipulator.addById(chr.Client, itemid, (short)count, "", expiration: period > 0 ? (chr.Client.CurrentServerContainer.GetCurrentTimeDateTimeOffSet().AddMinutes(period).ToUnixTimeMilliseconds()) : -1);
            chr.sendPacket(PacketCreator.getShowItemGain(itemid, (short)count, true));
        }
    }

    public override bool check(IPlayer chr, int? extSelection)
    {
        List<ItemInventoryType> gainList = new();
        List<ItemInventoryType> selectList = new();
        List<ItemInventoryType> randomList = new();

        List<int> allSlotUsed = new(5);
        for (byte i = 0; i < 5; i++)
        {
            allSlotUsed.Add(0);
        }

        foreach (ItemData item in items)
        {
            if (!canGetItem(item, chr))
            {
                continue;
            }

            InventoryType type = ItemConstants.getInventoryType(item.getId());
            if (item.getProp() != null)
            {
                Item toItem = new Item(item.getId(), 0, (short)item.getCount());

                if (item.getProp() < 0)
                {
                    selectList.Add(new(toItem, type));
                }
                else
                {
                    randomList.Add(new(toItem, type));
                }

            }
            else
            {
                // Make sure they can hold the item.
                Item toItem = new Item(item.getId(), 0, (short)item.getCount());
                gainList.Add(new(toItem, type));

                if (item.getCount() < 0)
                {
                    // Make sure they actually have the item.
                    int quantity = item.getCount() * -1;

                    int freeSlotCount = chr.getInventory(type).freeSlotCountById(item.getId(), quantity);
                    if (freeSlotCount == -1)
                    {
                        if (type.Equals(InventoryType.EQUIP) && chr.getInventory(InventoryType.EQUIPPED).countById(item.getId()) > quantity)
                        {
                            continue;
                        }

                        announceInventoryLimit(Collections.singletonList(item.getId()), chr);
                        return false;
                    }
                    else
                    {
                        int idx = type.getType() - 1;   // more slots available from the given items!
                        allSlotUsed[idx] -= freeSlotCount;
                    }
                }
            }
        }

        if (randomList.Count > 0)
        {
            int result;
            var c = chr.getClient();

            List<int> rndUsed = new(5);
            for (byte i = 0; i < 5; i++)
            {
                rndUsed.Add(allSlotUsed.get(i));
            }

            foreach (var it in randomList)
            {
                int idx = it.Type.getType() - 1;

                result = InventoryManipulator.checkSpaceProgressively(c, it.Item.getItemId(), it.Item.getQuantity(), "", rndUsed.get(idx), false);
                if (result % 2 == 0)
                {
                    announceInventoryLimit(Collections.singletonList(it.Item.getItemId()), chr);
                    return false;
                }

                allSlotUsed[idx] = Math.Max(allSlotUsed[idx], result >> 1);
            }
        }

        if (selectList.Count > 0)
        {
            var selected = selectList.get(extSelection ?? 0);
            gainList.Add(selected);
        }

        if (!canHold(chr, gainList))
        {
            List<int> gainItemids = new();
            foreach (var it in gainList)
            {
                gainItemids.Add(it.Item.getItemId());
            }

            announceInventoryLimit(gainItemids, chr);
            return false;
        }
        return true;
    }

    private void announceInventoryLimit(List<int> itemids, IPlayer chr)
    {
        if (!chr.canHoldUniques(itemids))
        {
            chr.dropMessage(1, "Please check if you already have a similar one-of-a-kind item in your inventory.");
            return;
        }

        chr.dropMessage(1, "Please check if you have enough space in your inventory.");
    }

    private bool canHold(IPlayer chr, List<ItemInventoryType> gainList)
    {
        List<int> toAddItemids = new();
        List<int> toAddQuantity = new();
        List<int> toRemoveItemids = new();
        List<int> toRemoveQuantity = new();

        foreach (var item in gainList)
        {
            Item it = item.Item;

            if (it.getQuantity() > 0)
            {
                toAddItemids.Add(it.getItemId());
                toAddQuantity.Add(it.getQuantity());
            }
            else
            {
                toRemoveItemids.Add(it.getItemId());
                toRemoveQuantity.Add(-1 * it.getQuantity());
            }
        }

        // thanks onechord for noticing quests unnecessarily giving out "full inventory" from quests that also takes items from players
        return chr.getAbstractPlayerInteraction().canHoldAllAfterRemoving(toAddItemids, toAddQuantity, toRemoveItemids, toRemoveQuantity);
    }

    private bool canGetItem(ItemData item, IPlayer chr)
    {
        if (item.getGender() != 2 && item.getGender() != chr.getGender())
        {
            return false;
        }

        if (item.getJob() > 0)
        {
            List<int> code = getJobBy5ByteEncoding(item.getJob());
            bool jobFound = false;
            foreach (int codec in code)
            {
                if (codec / 100 == chr.getJob().getId() / 100)
                {
                    jobFound = true;
                    break;
                }
            }
            return jobFound;
        }

        return true;
    }

    public bool restoreLostItem(IPlayer chr, int itemid)
    {
        if (!ItemInformationProvider.getInstance().isQuestItem(itemid))
        {
            return false;
        }

        // thanks danielktran (MapleHeroesD)
        foreach (ItemData item in items)
        {
            if (item.getId() == itemid)
            {
                int missingQty = item.getCount() - chr.countItem(itemid);
                if (missingQty > 0)
                {
                    if (!chr.canHold(itemid, missingQty))
                    {
                        chr.dropMessage(1, "Please check if you have enough space in your inventory.");
                        return false;
                    }

                    InventoryManipulator.addById(chr.Client, item.getId(), (short)missingQty);
                    log.Debug("Chr {CharacterId} obtained {ItemId}x {ItemQuantility} from questId {QuestId}", chr.getId(), itemid, missingQty, questID);
                }
                return true;
            }
        }

        return false;
    }

    private class ItemData
    {
        public int map, id, count, job, gender, period;
        private int? prop;

        public ItemData(int map, int id, int count, int? prop, int job, int gender, int period)
        {
            this.map = map;
            this.id = id;
            this.count = count;
            this.prop = prop;
            this.job = job;
            this.gender = gender;
            this.period = period;
        }

        public int getId()
        {
            return id;
        }

        public int getCount()
        {
            return count;
        }

        public int? getProp()
        {
            return prop;
        }

        public int getJob()
        {
            return job;
        }

        public int getGender()
        {
            return gender;
        }

        public int getPeriod()
        {
            return period;
        }
    }
}
