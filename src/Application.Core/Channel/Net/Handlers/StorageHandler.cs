/*
	This file is part of the OdinMS Maple Story NewServer
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
using Application.Core.Channel.ServerData;
using client.autoban;
using client.inventory;
using client.inventory.manipulator;
using Microsoft.Extensions.Logging;
using server;
using tools;

namespace Application.Core.Channel.Net.Handlers;

/**
 * @author Matze
 */
public class StorageHandler : ChannelHandlerBase
{
    readonly ILogger<StorageHandler> _logger;
    readonly AutoBanDataManager _autoBanManager;

    public StorageHandler(ILogger<StorageHandler> logger, AutoBanDataManager autoBanManager)
    {
        _logger = logger;
        _autoBanManager = autoBanManager;
    }

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        storageAction(p, c);
    }

    public void storageAction(InPacket p, IChannelClient c)
    {
        ItemInformationProvider ii = ItemInformationProvider.getInstance();
        var chr = c.OnlinedCharacter;
        Storage storage = chr.getStorage();
        string gmBlockedStorageMessage = "You cannot use the storage as a GM of this level.";

        byte mode = p.readByte();

        if (chr.getLevel() < 15)
        {
            chr.dropMessage(1, "You may only use the storage once you have reached level 15.");
            c.sendPacket(PacketCreator.enableActions());
            return;
        }

        if (c.tryacquireClient())
        {
            try
            {
                switch (mode)
                {
                    case 4:
                        { // Take out
                            sbyte type = p.ReadSByte();
                            sbyte slot = p.ReadSByte();
                            if (slot < 0 || slot > storage.getSlots())
                            {
                                // removal starts at zero
                                _autoBanManager.Alert(AutobanFactory.PACKET_EDIT, chr, chr.getName() + " tried to packet edit with storage.");
                                _logger.LogWarning("Chr {CharacterName} tried to work with storage slot {Slot}", chr.getName(), slot);
                                c.Disconnect(true, false);
                                return;
                            }

                            slot = storage.getSlot(InventoryTypeUtils.getByType(type), slot);
                            var item = storage.getItem(slot);

                            if (StorageProcessor.hasGMRestrictions(chr))
                            {
                                chr.dropMessage(1, gmBlockedStorageMessage);
                                _logger.LogInformation("GM {GM} blocked from using storage", chr);

                                chr.sendPacket(PacketCreator.enableActions());
                                return;
                            }

                            if (item != null)
                            {
                                if (!chr.CanHoldUniquesOnly(item.getItemId()))
                                {
                                    c.sendPacket(PacketCreator.getStorageError(0x0C));
                                    return;
                                }

                                int takeoutFee = storage.getTakeOutFee();
                                if (chr.getMeso() < takeoutFee)
                                {
                                    c.sendPacket(PacketCreator.getStorageError(0x0B));
                                    return;
                                }
                                else
                                {
                                    chr.gainMeso(-takeoutFee, false);
                                }

                                if (InventoryManipulator.checkSpace(c, item.getItemId(), item.getQuantity(), item.getOwner()))
                                {
                                    if (storage.takeOut(item))
                                    {
                                        KarmaManipulator.toggleKarmaFlagToUntradeable(item);
                                        InventoryManipulator.addFromDrop(c, item, false);

                                        _logger.LogDebug("Chr {CharacterName} took out {ItemQuantity}x {ItemName} ({ItemId})", 
                                            chr.getName(), 
                                            item.getQuantity(),
                                            ClientCulture.SystemCulture.GetItemName(item.getItemId()), 
                                            item.getItemId());

                                        storage.sendTakenOut(c, item.getInventoryType());
                                    }
                                    else
                                    {
                                        c.sendPacket(PacketCreator.enableActions());
                                        return;
                                    }
                                }
                                else
                                {
                                    c.sendPacket(PacketCreator.getStorageError(0x0A));
                                }
                            }
                            break;
                        }
                    case 5:
                        { // Store
                            short slot = p.readShort();
                            int itemId = p.readInt();
                            short quantity = p.readShort();
                            InventoryType invType = ItemConstants.getInventoryType(itemId);
                            Inventory inv = chr.getInventory(invType);
                            if (slot < 1 || slot > inv.getSlotLimit())
                            {
                                // player inv starts at one
                                _autoBanManager.Alert(AutobanFactory.PACKET_EDIT, chr, chr.getName() + " tried to packet edit with storage.");
                                _logger.LogWarning("Chr {ChracterName} tried to store item at slot {Slot}", c.OnlinedCharacter.getName(), slot);
                                c.Disconnect(true, false);
                                return;
                            }

                            if (StorageProcessor.hasGMRestrictions(chr))
                            {
                                chr.dropMessage(1, gmBlockedStorageMessage);
                                _logger.LogInformation("GM {GM} blocked from using storage", chr);
                                chr.sendPacket(PacketCreator.enableActions());
                                return;
                            }

                            if (quantity < 1)
                            {
                                c.sendPacket(PacketCreator.enableActions());
                                return;
                            }
                            if (storage.isFull())
                            {
                                c.sendPacket(PacketCreator.getStorageError(0x11));
                                return;
                            }
                            int storeFee = storage.getStoreFee();
                            if (chr.getMeso() < storeFee)
                            {
                                c.sendPacket(PacketCreator.getStorageError(0x0B));
                            }
                            else
                            {
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
                                            c.sendPacket(PacketCreator.enableActions());
                                            return;
                                        }

                                        if (ItemConstants.isRechargeable(itemId))
                                        {
                                            quantity = item.getQuantity();
                                        }

                                        InventoryManipulator.removeFromSlot(c, invType, slot, quantity, false);
                                    }
                                    else
                                    {
                                        c.sendPacket(PacketCreator.enableActions());
                                        return;
                                    }

                                    item = item.copy(); // thanks Robin Schulz & BHB88 for noticing a inventory glitch when storing items
                                }
                                finally
                                {
                                    inv.unlockInventory();
                                }

                                chr.gainMeso(-storeFee, false, true, false);

                                KarmaManipulator.toggleKarmaFlagToUntradeable(item);
                                item.setQuantity(quantity);

                                storage.store(item); // inside a critical section, "!(storage.isFull())" is still in effect...

                                _logger.LogDebug("Chr {CharacterName} stored {ItemQuantity}x {ItemName} ({ItemId})", 
                                    c.OnlinedCharacter.getName(), 
                                    item.getQuantity(),
                                    ClientCulture.SystemCulture.GetItemName(item.getItemId()), 
                                    item.getItemId());
                                storage.sendStored(c, ItemConstants.getInventoryType(itemId));
                            }
                            break;
                        }
                    case 6: // Arrange items
                        if (YamlConfig.config.server.USE_STORAGE_ITEM_SORT)
                        {
                            storage.arrangeItems(c);
                        }
                        c.sendPacket(PacketCreator.enableActions());
                        break;
                    case 7:
                        { // Mesos
                            int meso = p.readInt();
                            int storageMesos = storage.getMeso();
                            int playerMesos = chr.getMeso();

                            if (StorageProcessor.hasGMRestrictions(chr))
                            {
                                chr.dropMessage(1, gmBlockedStorageMessage);
                                _logger.LogInformation("GM {CharacterName} blocked from using storage", chr);
                                chr.sendPacket(PacketCreator.enableActions());
                                return;
                            }

                            if ((meso > 0 && storageMesos >= meso) || (meso < 0 && playerMesos >= -meso))
                            {
                                if (meso < 0 && (storageMesos - meso) < 0)
                                {
                                    meso = int.MinValue + storageMesos;
                                    if (meso < playerMesos)
                                    {
                                        c.sendPacket(PacketCreator.enableActions());
                                        return;
                                    }
                                }
                                else if (meso > 0 && (playerMesos + meso) < 0)
                                {
                                    meso = int.MaxValue - playerMesos;
                                    if (meso > storageMesos)
                                    {
                                        c.sendPacket(PacketCreator.enableActions());
                                        return;
                                    }
                                }
                                storage.setMeso(storageMesos - meso);
                                chr.gainMeso(meso, false, true, false);
                                _logger.LogDebug("Chr {CharacterName} {0} {meso} mesos", c.OnlinedCharacter.getName(), meso > 0 ? "took out" : "stored", Math.Abs(meso));
                                storage.sendMeso(c);
                            }
                            else
                            {
                                c.sendPacket(PacketCreator.enableActions());
                                return;
                            }
                            break;
                        }
                    case 8: // Close (unless the player decides to enter cash shop)
                        storage.close();
                        break;
                }
            }
            finally
            {
                c.releaseClient();
            }
        }
    }
}