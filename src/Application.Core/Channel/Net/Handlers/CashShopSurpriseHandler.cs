/*
    This file is part of the HeavenMS MapleStory NewServer
    Copyleft (L) 2016 - 2019 RonanLana

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


using Application.Core.Channel.Services;
using client.inventory;
using server;
using tools;

namespace Application.Core.Channel.Net.Handlers;



/**
 * @author RonanLana
 * @author Ponk
 */
public class CashShopSurpriseHandler : ChannelHandlerBase
{

    readonly ItemService _itemService;

    public CashShopSurpriseHandler(ItemService itemService)
    {
        _itemService = itemService;
    }

    public override Task HandlePacket(InPacket p, IChannelClient c)
    {
        if (c.tryacquireClient())
        {
            try
            {
                CashShop cs = c.OnlinedCharacter.getCashShop();
                if (!cs.isOpened())
                {
                    return Task.CompletedTask;
                }

                long cashId = p.readLong();
                var result = _itemService.OpenCashShopSurprise(cs, cashId);
                if (result == null)
                {
                    c.sendPacket(PacketCreator.onCashItemGachaponOpenFailed());
                    return Task.CompletedTask;
                }

                Item usedCashShopSurprise = result.usedCashShopSurprise;
                Item reward = result.reward;
                c.sendPacket(PacketCreator.onCashGachaponOpenSuccess(c.AccountEntity!.Id, usedCashShopSurprise.getCashId(),
                        usedCashShopSurprise.getQuantity(), reward, reward.getItemId(), reward.getQuantity(), true));
            }
            finally
            {
                c.releaseClient();
            }
        }
        return Task.CompletedTask;
    }
}
