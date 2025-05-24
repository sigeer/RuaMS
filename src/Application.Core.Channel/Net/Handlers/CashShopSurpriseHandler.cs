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


using client.inventory;
using net.packet;
using server;
using tools;

namespace Application.Core.Channel.Net.Handlers;



/**
 * @author RonanLana
 * @author Ponk
 */
public class CashShopSurpriseHandler : ChannelHandlerBase
{

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        CashShop cs = c.OnlinedCharacter.getCashShop();
        if (!cs.isOpened())
        {
            return;
        }

        long cashId = p.readLong();
        var result = cs.openCashShopSurprise(cashId);
        if (result == null)
        {
            c.sendPacket(PacketCreator.onCashItemGachaponOpenFailed());
            return;
        }

        Item usedCashShopSurprise = result.usedCashShopSurprise;
        Item reward = result.reward;
        c.sendPacket(PacketCreator.onCashGachaponOpenSuccess(c.AccountEntity!.Id, usedCashShopSurprise.getCashId(),
                usedCashShopSurprise.getQuantity(), reward, reward.getItemId(), reward.getQuantity(), true));
    }
}
