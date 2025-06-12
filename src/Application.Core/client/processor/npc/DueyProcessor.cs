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


using Application.Core.Duey;
using Application.Core.Game.Trades;
using Application.Core.model;
using Application.Shared.Items;
using client.autoban;
using client.inventory;
using client.inventory.manipulator;
using Microsoft.EntityFrameworkCore;
using server;
using tools;

namespace client.processor.npc;


/**
 * @author RonanLana - synchronization of Duey modules
 */
public class DueyProcessor
{
    private static ILogger log = LogFactory.GetLogger(LogType.Duey);



    public static void dueyRemovePackage(IChannelClient c, int packageid, bool playerRemove)
    {
        if (c.tryacquireClient())
        {
            try
            {
                c.CurrentServerContainer.Transport.RequestRemovePackage(packageid);
                c.sendPacket(PacketCreator.removeItemFromDuey(playerRemove, packageid));
            }
            finally
            {
                c.releaseClient();
            }
        }
    }

    public static void dueyClaimPackage(IChannelClient c, int packageId)
    {
        if (c.tryacquireClient())
        {
            try
            {
                try
                {
                    var dp = c.CurrentServer.Service.GetDueyPackageByPackageId(packageId);
                    if (dp == null)
                    {
                        c.sendPacket(PacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_RECV_UNKNOWN_ERROR.getCode()));
                        log.Warning("Chr {CharacterName} tried to receive namespace from duey with id {PackageId}", c.OnlinedCharacter.getName(), packageId);
                        return;
                    }

                    if (dp.isDeliveringTime())
                    {
                        c.sendPacket(PacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_RECV_UNKNOWN_ERROR.getCode()));
                        return;
                    }

                    var dpItem = dp.Item;
                    if (dpItem != null)
                    {
                        if (!c.OnlinedCharacter.canHoldMeso(dp.Mesos))
                        {
                            c.sendPacket(PacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_RECV_UNKNOWN_ERROR.getCode()));
                            return;
                        }

                        if (!InventoryManipulator.checkSpace(c, dpItem.getItemId(), dpItem.getQuantity(), dpItem.getOwner()))
                        {
                            int itemid = dpItem.getItemId();
                            if (ItemInformationProvider.getInstance().isPickupRestricted(itemid) && c.OnlinedCharacter.getInventory(ItemConstants.getInventoryType(itemid)).findById(itemid) != null)
                            {
                                c.sendPacket(PacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_RECV_RECEIVER_WITH_UNIQUE.getCode()));
                            }
                            else
                            {
                                c.sendPacket(PacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_RECV_NO_FREE_SLOTS.getCode()));
                            }

                            return;
                        }
                        else
                        {
                            InventoryManipulator.addFromDrop(c, dpItem, false);
                        }
                    }

                    c.OnlinedCharacter.gainMeso(dp.Mesos, false);

                    dueyRemovePackage(c, packageId, false);
                }
                catch (Exception e)
                {
                    log.Error(e.ToString());
                }
            }
            finally
            {
                c.releaseClient();
            }
        }
    }

    public static void dueySendTalk(IChannelClient c, bool quickDelivery)
    {
        if (c.tryacquireClient())
        {
            try
            {
                long timeNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                if (timeNow - c.OnlinedCharacter.getNpcCooldown() < YamlConfig.config.server.BLOCK_NPC_RACE_CONDT)
                {
                    c.sendPacket(PacketCreator.enableActions());
                    return;
                }
                c.OnlinedCharacter.setNpcCooldown(timeNow);

                if (quickDelivery)
                {
                    c.sendPacket(PacketCreator.sendDuey(0x1A, []));
                }
                else
                {
                    c.sendPacket(PacketCreator.sendDuey(0x8, c.OnlinedCharacter.GetDueyPackages()));
                }
            }
            finally
            {
                c.releaseClient();
            }
        }
    }
}

public enum DueyProcessorActions
{
    TOSERVER_RECV_ITEM = 0x00,
    TOSERVER_SEND_ITEM = 0x02,
    TOSERVER_CLAIM_PACKAGE = 0x04,
    TOSERVER_REMOVE_PACKAGE = 0x05,
    TOSERVER_CLOSE_DUEY = 0x07,
    TOCLIENT_OPEN_DUEY = 0x08,
    TOCLIENT_SEND_ENABLE_ACTIONS = 0x09,
    TOCLIENT_SEND_NOT_ENOUGH_MESOS = 0x0A,
    TOCLIENT_SEND_INCORRECT_REQUEST = 0x0B,
    TOCLIENT_SEND_NAME_DOES_NOT_EXIST = 0x0C,
    TOCLIENT_SEND_SAMEACC_ERROR = 0x0D,
    TOCLIENT_SEND_RECEIVER_STORAGE_FULL = 0x0E,
    TOCLIENT_SEND_RECEIVER_UNABLE_TO_RECV = 0x0F,
    TOCLIENT_SEND_RECEIVER_STORAGE_WITH_UNIQUE = 0x10,
    TOCLIENT_SEND_MESO_LIMIT = 0x11,
    TOCLIENT_SEND_SUCCESSFULLY_SENT = 0x12,
    TOCLIENT_RECV_UNKNOWN_ERROR = 0x13,
    TOCLIENT_RECV_ENABLE_ACTIONS = 0x14,
    TOCLIENT_RECV_NO_FREE_SLOTS = 0x15,
    TOCLIENT_RECV_RECEIVER_WITH_UNIQUE = 0x16,
    TOCLIENT_RECV_SUCCESSFUL_MSG = 0x17,
    TOCLIENT_RECV_PACKAGE_MSG = 0x1B
}

public static class ActionsExntesions
{
    public static byte getCode(this DueyProcessorActions a)
    {
        return (byte)a;
    }
}