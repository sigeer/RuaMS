/*
	This file is part of the OdinMS Maple Story Server
    Copyright (C) 2008 Patrick Huy <patrick.huy@frz.cc>
		       Matthias Butz <matze@odinms.de>
		       Jan Christian Meyer <vimes@odinms.de>

    Copyleft (L) 2016 - 2019 RonanLana (HeavenMS)

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


using Application.Core.Channel;
using Application.Core.Channel.DataProviders;
using Application.Core.Models;
using AutoMapper;
using client.inventory;
using client.inventory.manipulator;
using ItemProto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using tools;

namespace client.processor.npc;

/**
 * @author RonanLana - synchronization of Fredrick modules and operation results
 */
public class FredrickProcessor
{
    readonly ILogger<FredrickProcessor> _logger;
    readonly IMapper _mapper;
    readonly WorldChannelServer _server;

    public FredrickProcessor(ILogger<FredrickProcessor> logger, IMapper mapper, WorldChannelServer server)
    {
        _logger = logger;
        _mapper = mapper;
        _server = server;
    }

    private byte canRetrieveFromFredrick(IPlayer chr, int netMeso, List<Item> items)
    {
        if (!Inventory.checkSpot(chr, items))
        {
            if (chr.canHoldUniques(items.Select(x => x.getItemId()).ToList()))
            {
                return 0x22;
            }
            else
            {
                return 0x20;
            }
        }

        if (netMeso > 0)
        {
            if (!chr.canHoldMeso(netMeso))
            {
                return 0x1F;
            }
        }
        else
        {
            if (chr.getMeso() < -1 * netMeso)
            {
                return 0x21;
            }
        }

        return 0x0;
    }


    public PlayerShopLocalInfo LoadPlayerHiredMerchant(IPlayer chr)
    {
        var res = _server.Transport.LoadPlayerHiredMerchant(new ItemProto.GetPlayerHiredMerchantRequest { MasterId = chr.Id});

        return _mapper.Map<PlayerShopLocalInfo>(res);
    }

    public void fredrickRetrieveItems(IChannelClient c)
    {    
        // thanks Gustav for pointing out the dupe on Fredrick handling
        if (c.tryacquireClient())
        {
            try
            {
                var chr = c.OnlinedCharacter;

                try
                {
                    var res = LoadPlayerHiredMerchant(chr);
                    if (res.MapId > 0)
                    {
                        // 有正在营业的商店
                        return;
                    }

                    var items = _mapper.Map<List<Item>>(res.Items);

                    byte response = canRetrieveFromFredrick(chr, res.Mesos, items);
                    if (response != 0)
                    {
                        chr.sendPacket(PacketCreator.fredrickMessage(response));
                        return;
                    }

                    CommitRetrievedResponse commitRes = _server.Transport.CommitRetrievedFromFredrick(new CommitRetrievedRequest
                    {
                        OwnerId = chr.Id,
                    });
                    if (!commitRes.IsSuccess)
                    {

                        return;
                    }

                    chr.GainMeso(res.Mesos, false);

                    var commitRequest = new CommitRetrievedRequest
                    {
                        OwnerId = chr.Id,
                    };

                    foreach (var it in items)
                    {
                        InventoryManipulator.addFromDrop(chr.Client, it, false);
                        var itemName = ItemInformationProvider.getInstance().getName(it.getItemId());
                        _logger.LogDebug("Chr {CharacterName} gained {ItemQuantity}x {ItemName} ({CharacterId})", chr.getName(), it.getQuantity(), itemName, it.getItemId());
                    }

                    chr.sendPacket(PacketCreator.fredrickMessage(0x1E));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
            }
            finally
            {
                c.releaseClient();
            }
        }
    }
}
