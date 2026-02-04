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


using Application.Core.Channel.Net.Packets;
using Application.Core.Server;
using Application.Resources.Messages;
using Application.Templates.Npc;
using Application.Templates.Providers;
using Application.Templates.XmlWzReader.Provider;
using client.inventory;
using tools;

namespace server;



/**
 * @author Matze
 */
public class Storage : AbstractStorage
{
    private ILogger log;

    public int AccountId { get; set; }

    NpcTemplate? NpcTemplate { get; set; }
    public Storage(Player chr, int id, byte slots, int meso, Item[] items) : base(chr, slots, meso, items)
    {
        log = LogFactory.GetLogger(LogType.Storage);
        this.AccountId = id;
    }


    public bool TryGainSlots(int slots)
    {
        if (CanGainSlots(Slots))
        {
            this.Slots += (byte)slots;
            return true;
        }

        return false;
    }

    public override bool TakeOutItemCheck(Item item)
    {
        var fee = NpcTemplate?.TrunkGet ?? 0;
        if (Owner.getMeso() < fee)
        {
            Owner.sendPacket(StoragePacketCreator.getStorageError(0x0B));
            return false;
        }

        return base.TakeOutItemCheck(item);
    }

    public override bool StoreItemCheck(short slot, int itemId, short quantity)
    {
        var fee = NpcTemplate?.TrunkPut ?? 0;
        if (Owner.getMeso() < fee)
        {
            Owner.sendPacket(StoragePacketCreator.getStorageError(0x0B));
            return false;
        }

        return base.StoreItemCheck(slot, itemId, quantity);
    }

    public override void OnTakeOutSuccess(Item item)
    {
        if (NpcTemplate != null && NpcTemplate.TrunkGet > 0)
            Owner.GainMeso(-NpcTemplate.TrunkGet.Value, enableActions: true);
    }

    public override void OnStoreSuccess(short slot, int itemId, short quantity)
    {
        if (NpcTemplate != null && NpcTemplate.TrunkPut > 0)
            Owner.GainMeso(-NpcTemplate.TrunkPut.Value, enableActions: true);
    }

    public override void OpenStorage(int npcId)
    {
        if (Owner.getLevel() < 15)
        {
            Owner.Popup(nameof(ClientMessage.Storage_NeedLevel));
            Owner.sendPacket(PacketCreator.enableActions());
            return;
        }

        NpcTemplate = ProviderSource.Instance.GetProvider<NpcProvider>().GetItem(npcId);
        base.OpenStorage(npcId);
    }
}
