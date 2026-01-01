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
using tools;

namespace Application.Core.Channel.Net.Handlers;

/**
 * @author kevintjuh93 - :3
 */
public class RemoteStoreHandler : ChannelHandlerBase
{
    readonly AutoBanDataManager _autoBan;

    public RemoteStoreHandler(AutoBanDataManager autoBan)
    {
        _autoBan = autoBan;
    }

    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        var unknown = p.available();

        var chr = c.OnlinedCharacter;

        if (chr.getInventory(InventoryType.CASH).findById(ItemId.REMOTE_CONTROLLER) == null)
        {
            await _autoBan.Alert(AutobanFactory.ITEM_VAC, chr, $"没有道具 {ItemId.REMOTE_CONTROLLER} 却尝试远程打开雇佣商店");
            return;
        }

        var res = c.CurrentServerContainer.Transport.FindPlayerShopChannel(new ItemProto.SearchHiredMerchantChannelRequest { MasterId = chr.Id });
        if (res.Channel > 0)
        {
            if (res.Channel == chr.getClient().getChannel())
            {
                var hm = c.CurrentServer.PlayerShopManager.GetPlayerShop(PlayerShopType.HiredMerchant, chr.Id);
                hm!.VisitShop(chr);
            }
            else
            {
                c.sendPacket(PacketCreator.remoteChannelChange((byte)(res.Channel - 1)));
            }
            return;
        }
        else
        {
            chr.dropMessage(1, "你没有一个正在运营的雇佣商店.");
        }
        c.sendPacket(PacketCreator.enableActions());
        return;
    }
}
