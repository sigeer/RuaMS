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



using Application.Core.Channel;
using Application.Core.Channel.Net.Packets;
using Application.Core.Server;
using client.inventory;
using client.inventory.manipulator;
using tools;
using ZLinq;

/**
 * @author Matze
 * @author Ronan - inventory concurrency protection on storing items
 */
public class StorageProcessor
{
    static ILogger _logger = LogFactory.GetLogger(LogType.Storage);
    public static bool hasGMRestrictions(IPlayer character)
    {
        return character.isGM() && character.gmLevel() < YamlConfig.config.server.MINIMUM_GM_LEVEL_TO_USE_STORAGE;
    }

    public static void TakeOut(AbstractStorage storage, Item item)
    {
        if (!storage.TakeOutItemCheck(item))
            return;

        if (storage.RemoveItem(item))
        {
            KarmaManipulator.toggleKarmaFlagToUntradeable(item);
            InventoryManipulator.addFromDrop(storage.Owner.Client, item, false);
            storage.OnTakeOutSuccess(item);

            _logger.Debug("Chr {CharacterName} took out {ItemQuantity}x {ItemName} ({ItemId})",
                storage.Owner.getName(),
                item.getQuantity(),
                ClientCulture.SystemCulture.GetItemName(item.getItemId()),
                item.getItemId());

            storage.Owner.sendPacket(StoragePacketCreator.takeOutStorage(
                storage.Slots,
                item.getInventoryType(),
                storage.GetTypedItems(item.getInventoryType())));
            return;
        }
        else
        {
            storage.Owner.sendPacket(PacketCreator.enableActions());
            return;
        }
    }

    public static void Store(AbstractStorage storage, short slot, int itemId, short quantity)
    {
        if (!storage.StoreItemCheck(slot, itemId, quantity))
            return;

        InventoryType invType = ItemConstants.getInventoryType(itemId);
        Inventory inv = storage.Owner.getInventory(invType);

        Item? item;

        inv.lockInventory(); // thanks imbee for pointing a dupe within storage
        try
        {
            item = inv.getItem(slot);
            if (item != null && item.getItemId() == itemId
                    && (item.getQuantity() >= quantity || ItemConstants.isRechargeable(itemId)))
            {
                if (ItemId.isWeddingRing(itemId) || ItemId.isWeddingToken(itemId))
                {
                    storage.Owner.sendPacket(PacketCreator.enableActions());
                    return;
                }

                if (ItemConstants.isRechargeable(itemId))
                {
                    quantity = item.getQuantity();
                }

                item = item.copy();
                item.setQuantity(quantity);
                InventoryManipulator.removeFromSlot(storage.Owner.Client, invType, slot, quantity, false);
            }
            else
            {
                storage.Owner.sendPacket(PacketCreator.enableActions());
                return;
            }
        }
        finally
        {
            inv.unlockInventory();
        }

        storage.OnStoreSuccess(slot, itemId, quantity);

        KarmaManipulator.toggleKarmaFlagToUntradeable(item);

        storage.AddItem(item);

        _logger.Debug("Chr {CharacterName} stored {ItemQuantity}x {ItemName} ({ItemId})",
            storage.Owner.Name,
            quantity,
            ClientCulture.SystemCulture.GetItemName(item.getItemId()),
            item.getItemId());
        storage.Owner.sendPacket(StoragePacketCreator.storeStorage(
            storage.Slots,
            invType,
            storage.GetTypedItems(invType)));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="storage"></param>
    /// <param name="chr"></param>
    /// <param name="meso">大于0取出，小于0存入</param>
    public static void SetMeso(AbstractStorage storage, int meso)
    {
        if ((meso > 0 && storage.TakeOutMesoCheck(meso)) || (meso < 0 && storage.StoreMesoCheck(-meso)))
        {
            if (meso < 0 && (storage.Meso - meso) < 0)
            {
                meso = int.MinValue + storage.Meso;
                if (meso < storage.Owner.getMeso())
                {
                    storage.Owner.sendPacket(PacketCreator.enableActions());
                    return;
                }
            }
            else if (meso > 0 && (storage.Owner.getMeso() + meso) < 0)
            {
                meso = int.MaxValue - storage.Owner.getMeso();
                if (meso > storage.Meso)
                {
                    storage.Owner.sendPacket(PacketCreator.enableActions());
                    return;
                }
            }
            storage.Meso -= meso;

            storage.Owner.GainMeso(meso, false, true, false);
            _logger.Debug("Chr {CharacterName} {0} {meso} mesos", storage.Owner.Name, meso > 0 ? "took out" : "stored", Math.Abs(meso));
            storage.UpdateMeso();
        }
        else
        {
            storage.Owner.sendPacket(PacketCreator.enableActions());
            return;
        }
    }
}
