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
using Application.Core.Channel.Net.Packets;
using Application.Templates.Item;
using Application.Templates.StatEffectProps;
using client.inventory.manipulator;
using System.ComponentModel.DataAnnotations;
using tools;

namespace Application.Core.Channel.Net.Handlers;



/**
 * @author Jay Estrella
 * @author kevintjuh93
 */
public class ItemRewardHandler : UseItemHandlerBase
{
    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        var slot = p.readShort();
        var itemId = p.readInt(); // will load from xml I don't care.

        if (!TryGetItem(c.OnlinedCharacter, slot, itemId, 1, out var it))
        {
            return;
        }

        if (it.SourceTemplate is not IStatEffectReward template || template.Reward.Length == 0)
        {
            return;
        }

        // 已持有的唯一道具不会再获得，先剔除，避免抽中后 GainItem 静默失败但仍消耗奖励箱
        var pool = template.Reward.Where(x => c.OnlinedCharacter.CanHoldUniquesOnly(x.ItemID)).ToArray();
        if (pool.Length == 0)
        {
            await c.SendPacket(MessagePacket.CannotGetNewItem());
            return;
        }

        ItemInformationProvider ii = ItemInformationProvider.getInstance();
        var invSlotRequired = pool.GroupBy(x => ItemConstants.getInventoryType(x.ItemID))
            .ToDictionary(x => x.Key, x => x.Max(y => (int)Math.Ceiling((float)y.Count / ii.getSlotMax(c, y.ItemID))));

        if (!c.OnlinedCharacter.CanHold(invSlotRequired))
        {
            await c.SendPacket(MessagePacket.CannotGetNewItem());
            return;
        }

        var reward = new LotteryMachine<RewardData, int>(pool, x => x.Prob).GetRandomItem();

        await c.OnlinedCharacter.GainItem(reward.ItemID, reward.Count, expires: reward.Period * 60 * 60 * 10);
        await InventoryManipulator.removeById(c, InventoryType.USE, itemId, 1, false, false);

        if (!string.IsNullOrEmpty(reward.Effect))
        {
            await c.OnlinedCharacter.SendPacket(EffectPacket.UIEffect(reward.Effect));
        }

        if (!string.IsNullOrEmpty(reward.WorldMessage))
        {
            string msg = reward.WorldMessage;
            msg = msg.Replace("/name", c.OnlinedCharacter.getName());
            msg = msg.Replace("/item", c.CurrentCulture.GetItemName(reward.ItemID));
            c.CurrentServer.NodeService.SendDropMessage(6, msg);
        }

        await c.SendPacket(PacketCreator.enableActions());
    }
}
