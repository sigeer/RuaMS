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


using Application.Core.Channel.Net.Packets;
using Application.Core.Channel.ServerData;
using Application.Resources.Messages;
using Application.Shared.Items;
using tools;

namespace Application.Core.Channel.Net.Handlers;

/// <summary>
/// 进入拍卖行（MTS / ITC）
/// </summary>
public class EnterMTSHandler : ChannelHandlerBase
{
    readonly MTSManager _mtsManager;

    public EnterMTSHandler(MTSManager mtsManager)
    {
        _mtsManager = mtsManager;
    }

    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        if (!YamlConfig.config.server.USE_MTS)
        {
            await c.SendPacket(PacketCreator.enableActions());
            return;
        }

        var mc = c.OnlinedCharacter;
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

        await mc.CancelAllBuffs();
        mc.cancelAllDebuffs();
        await mc.forfeitExpirableQuests();

        await mc.SendPacket(MTSPacketCreator.OpenMTS(mc.Client));
        mc.getCashShop().open(true);

        await _mtsManager.Query(mc, MTSConstants.SALE_TAB, 0, 0);
    }
}