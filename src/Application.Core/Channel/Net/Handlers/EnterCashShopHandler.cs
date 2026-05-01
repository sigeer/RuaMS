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



using Application.Core.Channel.Commands;
using Application.Core.Channel.Services;
using Application.Resources.Messages;
using Microsoft.Extensions.Logging;
using tools;

namespace Application.Core.Channel.Net.Handlers;

/**
 * @author Flav
 */
public class EnterCashShopHandler : ChannelHandlerBase
{

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        c.OnlinedCharacter.MapModel.Send(async m =>
        {
            var mc = m.getCharacterById(c.OnlinedCharacter.Id);
            if (mc == null)
            {
                return;
            }

            if (mc.cannotEnterCashShop())
            {
                mc.sendPacket(PacketCreator.enableActions());
                return;
            }

            if (mc.getEventInstance() != null)
            {
                mc.Pink(nameof(ClientMessage.CashShop_CannotEnter_WithEventInstance));
                mc.sendPacket(PacketCreator.enableActions());
                return;
            }

            if (MiniDungeonInfo.isDungeonMap(mc.getMapId()))
            {
                mc.Pink(nameof(ClientMessage.ChangeChannel_MiniDungeon));
                mc.sendPacket(PacketCreator.enableActions());
                return;
            }

            if (mc.getCashShop().isOpened())
            {
                return;
            }

            await mc.SyncCharAsync(trigger: Shared.Events.SyncCharacterTrigger.EnterCashShop);

            mc.closePlayerInteractions();
            mc.closePartySearchInteractions();

            mc.unregisterChairBuff();
            mc.Client.CurrentServer.NodeService.DataService.SaveBuff(mc);

            mc.Client.CurrentServer.EnterExtralWorld(mc);

            mc.cancelAllBuffs(true);
            mc.cancelAllDebuffs();
            mc.forfeitExpirableQuests();

            mc.sendPacket(PacketCreator.openCashShop(mc.Client, false));
            mc.sendPacket(PacketCreator.showCashInventory(mc.Client));
            mc.sendPacket(PacketCreator.showGifts(mc.Client.CurrentServer.NodeService.ItemService.LoadPlayerGifts(mc)));
            mc.sendPacket(PacketCreator.showWishList(mc, false));
            mc.sendPacket(PacketCreator.showCash(mc));

            mc.getCashShop().open(true);
        });
    }
}
