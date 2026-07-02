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
                await mc.Pink(nameof(ClientMessage.CashShop_CannotEnter_WithEventInstance));
                await mc.SendPacket(PacketCreator.enableActions());
                return;
            }

            if (MiniDungeonInfo.isDungeonMap(mc.getMapId()))
            {
                await mc.Pink(nameof(ClientMessage.ChangeChannel_MiniDungeon));
                await mc.SendPacket(PacketCreator.enableActions());
                return;
            }

            if (mc.getCashShop().isOpened())
            {
                return;
            }

            await mc.SyncCharAsync(trigger: Shared.Events.SyncCharacterTrigger.EnterCashShop);

            await mc.closePlayerInteractions();
            mc.closePartySearchInteractions();

            await mc.unregisterChairBuff();
            mc.Client.CurrentServer.NodeService.DataService.SaveBuff(mc);

            await mc.Client.CurrentServer.EnterExtralWorld(mc);

            await mc.cancelAllBuffs(true);
            mc.cancelAllDebuffs();
            await mc.forfeitExpirableQuests();

            await mc.SendPacket(PacketCreator.openCashShop(mc.Client, false));
            await mc.SendPacket(PacketCreator.showCashInventory(mc.Client));
            await mc.SendPacket(PacketCreator.showGifts(mc.Client.CurrentServer.NodeService.ItemService.LoadPlayerGifts(mc)));
            await mc.SendPacket(PacketCreator.showWishList(mc, false));
            await mc.SendPacket(PacketCreator.showCash(mc));

            mc.getCashShop().open(true);
        });
    }
}
