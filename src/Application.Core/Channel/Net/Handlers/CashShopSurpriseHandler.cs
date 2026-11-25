/*
    This file is part of the HeavenMS MapleStory NewServer
    Copyleft (L) 2016 - 2019 RonanLana

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/


using Application.Core.Channel.ServerData;
using Application.Core.Server;
using client.inventory;
using tools;

namespace Application.Core.Channel.Net.Handlers;



/**
 * @author RonanLana
 * @author Ponk
 */
public class CashShopSurpriseHandler : ChannelHandlerBase
{

    readonly CashShopManager _cashShopManager;

    public CashShopSurpriseHandler(CashShopManager cashShopManager)
    {
        _cashShopManager = cashShopManager;
    }

    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        {
            await c.tryacquireClient();
            try
            {
                if (!c.OnlinedCharacter.IsInExtraServer())
                {
                    return;
                }

                long cashId = p.readLong();
                var result = await _cashShopManager.OpenCashShopSurpriseSynced(c, cashId);
                if (result == null)
                {
                    await c.SendPacket(PacketCreator.onCashItemGachaponOpenFailed());
                    return;
                }

                await c.SendPacket(PacketCreator.onCashGachaponOpenSuccess(c.AccountEntity!.Id, result.Used.UniqueId,
                        result.RemainingQuantity, result.Reward, result.Reward.Itemid, result.Reward.Quantity, true));
            }
            finally
            {
                c.releaseClient();
            }
        }
    }
}
