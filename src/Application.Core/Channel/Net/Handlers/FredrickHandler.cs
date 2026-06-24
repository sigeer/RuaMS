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


using Application.Core.Channel.Net.Packets;
using Application.Core.Channel.Services;

namespace Application.Core.Channel.Net.Handlers;


public class FredrickHandler : ChannelHandlerBase
{
    readonly PlayerShopService _service;
    public FredrickHandler(PlayerShopService service)
    {
        _service = service;
    }

    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        byte operation = p.readByte();

        switch (operation)
        {
            case 0x19: //Will never come...
                // await c.SendPacket(PacketCreator.getFredrick((byte) 0x24));
                Log.Logger.Debug("FredrickHandler - 0x19");
                break;
            case 0x1A:
                // sub_79B21A
                var data = _service.LoadPlayerHiredMerchant(c.OnlinedCharacter);
                await c.OnlinedCharacter.SendPacket(FredrickPackets.GetFredrickFeePrompt(data.FeePercentage, data.FeeMeso));
                break;
            case 0x1B:
                // sub_79B27D
                await _service.FredrickRetrieveItems(c);
                break;
            case 0x1C: //Exit
                // sub_79A090
                break;
            default:
                break;
        }
    }
}
