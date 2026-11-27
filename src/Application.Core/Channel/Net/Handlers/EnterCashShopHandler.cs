/*
	This file is part of the OdinMS Maple Story NewServer
    Copyright (C) 2008 Patrick Huy <patrick.huy@frz.cc>
                       Matthias Butz <matze@odinms.de>
                       Jan Christian Meyer <vimes@odinms.de>

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License version 3
    as published by the Free Software Foundation. You may not use, modify
    or distribute this program under any other version of the
    GNU Affero General Public License.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/



using Application.Core.Game.Trades;
using Application.Resources.Messages;
using tools;

namespace Application.Core.Channel.Net.Handlers;

/**
 * @author Flav
 */
public class EnterCashShopHandler : ChannelHandlerBase
{

    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        await c.OnlinedCharacter.MapModel.Send(async m =>
        {
            var mc = m.getCharacterById(c.OnlinedCharacter.Id);
            if (mc == null)
            {
                return;
            }

            if (mc.cannotEnterCashShop())
            {
                await mc.SendPacket(PacketCreator.enableActions());
                return;
            }

            if (mc.getEventInstance() != null)
            {
                await mc.Pink(ClientMessage.CashShop_CannotEnter_WithEventInstance);
                await mc.SendPacket(PacketCreator.enableActions());
                return;
            }

            if (MiniDungeonInfo.isDungeonMap(mc.getMapId()))
            {
                await mc.Pink(ClientMessage.ChangeChannel_MiniDungeon);
                await mc.SendPacket(PacketCreator.enableActions());
                return;
            }

            if (mc.IsInExtraServer())
            {
                return;
            }

            if (!await c.EnterTradeServer(new CashShopServer()))
            {
                // 没能脱离世界就别把商城界面打开，否则玩家卡在「界面已开、人还在图里」的状态
                await c.SendPacket(PacketCreator.enableActions());
                return;
            }

            // 余额、现金仓、格数、愿望单都在中心服，进商城先整体查一次
            var cashShopManager = c.CurrentServer.NodeService.CashShopManager;
            var cashState = cashShopManager.GetState(c);

            await c.SendPacket(PacketCreator.openCashShop(c, false));
            await c.SendPacket(PacketCreator.showCashInventory(c.AccountId, cashState));
            await c.SendPacket(PacketCreator.showGifts(await cashShopManager.LoadPlayerGifts(c)));
            await c.SendPacket(PacketCreator.showWishList(cashState.WishList, false));
            await c.SendPacket(PacketCreator.showCash(
                cashState.Cash?.NxCredit ?? 0,
                cashState.Cash?.MaplePoint ?? 0,
                cashState.Cash?.NxPrepaid ?? 0));
        });
    }
}
