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

using Application.Core.Channel.ServerData;
using client.autoban;
using Microsoft.Extensions.Logging;
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

    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        await storageAction(p, c);
    }

    public async Task storageAction(InPacket p, IChannelClient c)
    {
        var chr = c.OnlinedCharacter;

        byte mode = p.readByte();

        var storage = chr.CurrentStorage;
        if (storage == null)
        {
            await c.SendPacket(PacketCreator.enableActions());
            return;
        }
        {
            await c.tryacquireClient();
            try
            {
                switch (mode)
                {
                    case 4:
                        { // Take out
                            sbyte type = p.ReadSByte();
                            sbyte storageSlot = p.ReadSByte();
                            if (storageSlot < 0 || storageSlot > storage.Slots)
                            {
                                // removal starts at zero
                                _autoBanManager.Alert(AutobanFactory.PACKET_EDIT, chr, chr.getName() + " tried to packet edit with storage.");
                                _logger.LogWarning("Chr {CharacterName} tried to work with storage slot {Slot}", chr.getName(), storageSlot);
                                await c.Disconnect(true, false);
                                return;
                            }

                            var item = storage.GetItemByTypedSlot((InventoryType)type, storageSlot);
                            if (item != null)
                            {
                                await StorageProcessor.TakeOut(storage, item);
                            }
                            break;
                        }
                    case 5:
                        { // Store
                            short invSlot = p.readShort();
                            int itemId = p.readInt();
                            short quantity = p.readShort();
                            InventoryType invType = ItemConstants.getInventoryType(itemId);
                            var inv = chr.getInventory(invType);
                            if (invSlot < 1 || invSlot > inv.getSlotLimit())
                            {
                                // player inv starts at one
                                _autoBanManager.Alert(AutobanFactory.PACKET_EDIT, chr, chr.getName() + " tried to packet edit with storage.");
                                _logger.LogWarning("Chr {ChracterName} tried to store item at slot {Slot}", c.OnlinedCharacter.getName(), invSlot);
                                await c.Disconnect(true, false);
                                return;
                            }

                            await StorageProcessor.Store(storage, invSlot, itemId, quantity);
                            break;
                        }
                    case 6: // Arrange items
                        if (YamlConfig.config.server.USE_STORAGE_ITEM_SORT)
                        {
                            await storage.ArrangeItems();
                        }
                        await c.SendPacket(PacketCreator.enableActions());
                        break;
                    case 7:
                        { // Mesos
                            int meso = p.readInt();
                            await StorageProcessor.SetMeso(storage, meso);
                            break;
                        }
                    case 8: // Close (unless the player decides to enter cash shop)
                        chr.CurrentStorage = null;
                        await c.SendPacket(PacketCreator.enableActions());
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