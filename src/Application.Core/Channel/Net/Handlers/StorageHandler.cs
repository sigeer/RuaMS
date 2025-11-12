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
using Application.Resources.Messages;
using client.autoban;
using client.inventory;
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

        if (StorageProcessor.hasGMRestrictions(chr))
        {
            chr.dropMessage(1, "You cannot use the storage as a GM of this level.");
            _logger.LogInformation("GM {GM} blocked from using storage", chr);
            chr.sendPacket(PacketCreator.enableActions());
            return;
        }

        byte mode = p.readByte();

        if (chr.getLevel() < 15)
        {
            chr.Popup(nameof(ClientMessage.Storage_NeedLevel));
            c.sendPacket(PacketCreator.enableActions());
            return;
        }

        Storage storage = chr.getStorage();
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

                            var item = storage.GetItemBySlot(slot);
                            if (item != null)
                            {
                                StorageProcessor.TakeOut(storage, item);
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

                            StorageProcessor.Store(storage, slot, itemId, quantity);
                            break;
                        }
                    case 6: // Arrange items
                        if (YamlConfig.config.server.USE_STORAGE_ITEM_SORT)
                        {
                            storage.ArrangeItems();
                        }
                        c.sendPacket(PacketCreator.enableActions());
                        break;
                    case 7:
                        { // Mesos
                            int meso = p.readInt();
                            StorageProcessor.SetMeso(storage, meso);
                            break;
                        }
                    case 8: // Close (unless the player decides to enter cash shop)
                        c.sendPacket(PacketCreator.enableActions());
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