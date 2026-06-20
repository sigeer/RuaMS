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


using Application.Core.Game.Trades;
using client.inventory.manipulator;
using Microsoft.Extensions.Logging;
using tools;

namespace Application.Core.Channel.Net.Handlers;

public class ChangeMapHandler : ChannelHandlerBase
{
    readonly ILogger<ChangeMapHandler> _logger;

    public ChangeMapHandler(ILogger<ChangeMapHandler> logger)
    {
        _logger = logger;
    }

    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        var chr = c.OnlinedCharacter;

        if (chr.isChangingMaps() || chr.isBanned())
        {
            if (chr.isChangingMaps())
            {
                _logger.LogWarning("Chr {CharacterName} got stuck when changing maps. Last visited mapids: {LastVisitedMapId}", chr.getName(), chr.getLastVisitedMapids());
            }

            await c.SendPacket(PacketCreator.enableActions());
            return;
        }

        await chr.closeTrade(TradeResult.UNSUCCESSFUL_ANOTHER_MAP);

        bool enteringMapFromCashShop = p.available() == 0;
        if (enteringMapFromCashShop)
        {
            await c.LeaveCashShop();
            return;
        }

        if (chr.getCashShop().isOpened())
        {
            await c.Disconnect(false, false);
            return;
        }

        try
        {
            p.readByte(); // 1 = from dying 0 = regular portals
            int targetMapId = p.readInt();
            string portalName = p.readString();
            var portal = chr.getMap().getPortal(portalName);
            p.readByte();
            bool wheel = p.readByte() > 0;

            bool chasing = p.readByte() == 1 && chr.isGM() && p.available() == 2 * sizeof(int);
            if (chasing)
            {
                chr.setChasing(true);
                chr.setPosition(new Point(p.readInt(), p.readInt()));
            }

            if (targetMapId != -1)
            {
                if (!chr.isAlive())
                {
                    var map = chr.getMap();
                    if (wheel && chr.haveItemWithId(ItemId.WHEEL_OF_FORTUNE, false))
                    {
                        // thanks lucasziron (lziron) for showing revivePlayer() triggering by Wheel

                        await InventoryManipulator.removeById(c, InventoryType.CASH, ItemId.WHEEL_OF_FORTUNE, 1, true, false);
                        await chr.SendPacket(PacketCreator.showWheelsLeft(chr.getItemQuantity(ItemId.WHEEL_OF_FORTUNE)));

                        await chr.UpdateStatsChunk(async () =>
                        {
                            await chr.SetHP(50);
                        });
                        await chr.changeMap(map, map.findClosestPlayerSpawnpoint(chr.getPosition()));
                    }
                    else
                    {
                        bool executeStandardPath = true;
                        if (chr.getEventInstance() != null)
                        {
                            executeStandardPath = await chr.getEventInstance()!.revivePlayer(chr);
                        }
                        if (executeStandardPath)
                        {
                            await chr.respawn(map.getReturnMapId());
                        }
                    }
                }
                else
                {
                    if (chr.isGM())
                    {
                        var to = await chr.getWarpMap(targetMapId);
                        await chr.changeMap(to, to.getPortal(0));
                    }
                    else
                    {
                        int divi = chr.getMapId() / 100;
                        bool warp = false;
                        if (divi == 0)
                        {
                            if (targetMapId == 10000)
                            {
                                warp = true;
                            }
                        }
                        else if (divi == 20100)
                        {
                            if (targetMapId == MapId.LITH_HARBOUR)
                            {
                                await c.SendPacket(PacketCreator.lockUI(false));
                                await c.SendPacket(PacketCreator.disableUI(false));
                                warp = true;
                            }
                        }
                        else if (divi == 9130401)
                        { // Only allow warp if player is already in Intro map, or else = hack
                            if (targetMapId == MapId.EREVE || targetMapId / 100 == 9130401)
                            { // Cygnus introduction
                                warp = true;
                            }
                        }
                        else if (divi == 9140900)
                        { // Aran Introduction
                            if (targetMapId == MapId.ARAN_TUTO_2 || targetMapId == MapId.ARAN_TUTO_3 || targetMapId == MapId.ARAN_TUTO_4 || targetMapId == MapId.ARAN_INTRO)
                            {
                                warp = true;
                            }
                        }
                        else if (divi / 10 == 1020)
                        { // Adventurer movie clip Intro
                            if (targetMapId == 1020000)
                            {
                                warp = true;
                            }
                        }
                        else if (divi / 10 >= 980040 && divi / 10 <= 980045)
                        {
                            if (targetMapId == MapId.WITCH_TOWER_ENTRANCE)
                            {
                                warp = true;
                            }
                        }
                        if (warp)
                        {
                            var to = await chr.getWarpMap(targetMapId);
                            await chr.changeMap(to, to.getPortal(0));
                        }
                    }
                }
            }

            if (portal != null && !portal.getPortalStatus())
            {
                await c.SendPacket(PacketCreator.BlockMapMessage(1));
                await c.SendPacket(PacketCreator.enableActions());
                return;
            }

            if (chr.getMapId() == MapId.FITNESS_EVENT_LAST)
            {
                chr.Fitness?.resetTimes();
            }
            else if (chr.getMapId() == MapId.OLA_EVENT_LAST_1 || chr.getMapId() == MapId.OLA_EVENT_LAST_2)
            {
                chr.Ola?.resetTimes();
            }

            if (portal != null)
            {
                if (portal.getPosition().distanceSq(chr.getPosition()) > 400000)
                {
                    await c.SendPacket(PacketCreator.enableActions());
                    return;
                }

                await portal.enterPortal(c);
            }
            else
            {
                await c.SendPacket(PacketCreator.enableActions());
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
        }

    }
}