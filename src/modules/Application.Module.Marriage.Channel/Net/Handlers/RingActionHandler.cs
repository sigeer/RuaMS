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


using Application.Core.Channel.Net;
using Application.Core.Client;
using Application.Core.Managers;
using Application.Shared.Constants.Inventory;
using Application.Shared.Constants.Item;
using Application.Shared.Net;
using client.inventory;
using client.inventory.manipulator;
using Microsoft.Extensions.Logging;
using tools;

namespace Application.Module.Marriage.Channel.Net.Handlers;


/**
 * @author Jvlaple
 * @author Ronan - major overhaul on Ring handling mechanics
 * @author Drago (Dragohe4rt) - on Wishlist
 */
public class RingActionHandler : ChannelHandlerBase
{
    readonly ILogger<RingActionHandler> _logger;
    readonly WeddingManager _weddingManager;
    readonly MarriageManager _marriageManager;

    public RingActionHandler(ILogger<RingActionHandler> logger, WeddingManager weddingManager, MarriageManager marriageManager)
    {
        _logger = logger;
        _weddingManager = weddingManager;
        _marriageManager = marriageManager;
    }

    private static int getEngagementBoxId(int useItemId)
    {
        return useItemId switch
        {
            ItemId.ENGAGEMENT_BOX_MOONSTONE => ItemId.EMPTY_ENGAGEMENT_BOX_MOONSTONE,
            ItemId.ENGAGEMENT_BOX_STAR => ItemId.EMPTY_ENGAGEMENT_BOX_STAR,
            ItemId.ENGAGEMENT_BOX_GOLDEN => ItemId.EMPTY_ENGAGEMENT_BOX_GOLDEN,
            ItemId.ENGAGEMENT_BOX_SILVER => ItemId.EMPTY_ENGAGEMENT_BOX_SILVER,
            _ => ItemId.CARAT_RING_BASE + (useItemId - ItemId.CARAT_RING_BOX_BASE),
        };
    }

    public void sendEngageProposal(IChannelClient c, string name, int itemid)
    {
        int newBoxId = getEngagementBoxId(itemid);

        var source = c.OnlinedCharacter;

        if (source.getLevel() < 50)
        {
            source.dropMessage(1, "You can only propose being level 50 or higher.");
            source.sendPacket(WeddingPackets.OnMarriageResult(0));
            return;
        }

        var marriageInfo = _marriageManager.GetPlayerMarriageInfo(source.Id);
        if (marriageInfo?.Status == Common.Models.MarriageStatusEnum.Married)
        {
            source.dropMessage(1, "You're already married!");
            source.sendPacket(WeddingPackets.OnMarriageResult(0));
            return;
        }

        if (marriageInfo?.Status == Common.Models.MarriageStatusEnum.Engaged)
        {
            source.dropMessage(1, "You're already engaged!");
            source.sendPacket(WeddingPackets.OnMarriageResult(0));
            return;
        }

        var target = c.CurrentServer.Players.getCharacterByName(name);
        if (target == null)
        {
            source.dropMessage(1, "Unable to find " + name + " on this channel.");
            source.sendPacket(WeddingPackets.OnMarriageResult(0));
            return;
        }
        if (target == source)
        {
            source.dropMessage(1, "You can't engage yourself.");
            source.sendPacket(WeddingPackets.OnMarriageResult(0));
            return;
        }
        else if (target.getLevel() < 50)
        {
            source.dropMessage(1, "You can only propose to someone level 50 or higher.");
            source.sendPacket(WeddingPackets.OnMarriageResult(0));
            return;
        }
        else if (!target.getMap().Equals(source.getMap()))
        {
            source.dropMessage(1, "Make sure your partner is on the same map!");
            source.sendPacket(WeddingPackets.OnMarriageResult(0));
            return;
        }
        else if (!source.haveItem(itemid) || itemid < ItemId.ENGAGEMENT_BOX_MIN || itemid > ItemId.ENGAGEMENT_BOX_MAX)
        {
            source.sendPacket(WeddingPackets.OnMarriageResult(0));
            return;
        }

        marriageInfo = _marriageManager.GetPlayerMarriageInfo(target.Id);
        if (marriageInfo?.Status == Common.Models.MarriageStatusEnum.Married)
        {
            source.dropMessage(1, "The player is already married!");
            source.sendPacket(WeddingPackets.OnMarriageResult(0));
            return;
        }

        if (marriageInfo?.Status == Common.Models.MarriageStatusEnum.Engaged)
        {
            source.dropMessage(1, "The player is already engaged!");
            source.sendPacket(WeddingPackets.OnMarriageResult(0));
            return;
        }
        else if (_weddingManager.HasWeddingRing(target))
        {
            source.dropMessage(1, "The player already holds a marriage ring...");
            source.sendPacket(WeddingPackets.OnMarriageResult(0));
            return;
        }
        else if (_weddingManager.HasWeddingRing(source))
        {
            source.dropMessage(1, "You can't propose while holding a marriage ring!");
            source.sendPacket(WeddingPackets.OnMarriageResult(0));
            return;
        }
        else if (target.getGender() == source.getGender())
        {
            source.dropMessage(1, "You may only propose to a " + (source.getGender() == 1 ? "male" : "female") + "!");
            source.sendPacket(WeddingPackets.OnMarriageResult(0));
            return;
        }
        else if (!InventoryManipulator.checkSpace(c, newBoxId, 1, ""))
        {
            source.dropMessage(5, "You don't have a ETC slot available right now!");
            source.sendPacket(WeddingPackets.OnMarriageResult(0));
            return;
        }
        else if (!InventoryManipulator.checkSpace(target.Client, newBoxId + 1, 1, ""))
        {
            source.dropMessage(5, "The girl you proposed doesn't have a ETC slot available right now.");
            source.sendPacket(WeddingPackets.OnMarriageResult(0));
            return;
        }

        source.setMarriageItemId(itemid);
        target.sendPacket(WeddingPackets.onMarriageRequest(source.getName(), source.getId()));
    }


    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        byte mode = p.readByte();
        string name;
        byte slot;
        switch (mode)
        {
            case 0: // Send Proposal
                sendEngageProposal(c, p.readString(), p.readInt());
                break;

            case 1: // Cancel Proposal
                if (c.OnlinedCharacter.MarriageItemId / 1000000 != 4)
                {
                    c.OnlinedCharacter.MarriageItemId = -1;
                }
                break;

            case 2:
                // Accept/Deny Proposal
                bool accepted = p.readByte() > 0;
                name = p.readString();

                // 这个是否可以改成marriage item id？
                int id = p.readInt();

                var source = c.CurrentServer.Players.getCharacterByName(name);
                var target = c.OnlinedCharacter;

                if (source == null || !source.IsOnlined)
                {
                    target.sendPacket(PacketCreator.enableActions());
                    return;
                }

                int itemid = source.MarriageItemId;
                if (source.getId() != id || itemid <= 0 || !source.haveItem(itemid) || !source.isAlive() || !target.isAlive())
                {
                    target.sendPacket(PacketCreator.enableActions());
                    return;
                }

                if (accepted)
                {
                    _weddingManager.AcceptProposal(source, target);
                }
                else
                {
                    source.dropMessage(1, "She has politely declined your engagement request.");
                    source.sendPacket(WeddingPackets.OnMarriageResult(0));

                    source.setMarriageItemId(-1);
                }
                break;

            case 3:
                // Break Engagement
                // 根据物品来区分解除的关系？
                var removedItem = p.readInt();
                var unknown = p.available();

                _weddingManager.BreakMarriageRing(c.OnlinedCharacter);
                break;

            case 5:
                {
                    // Invite %s to Wedding
                    name = p.readString();
                    int marriageId = p.readInt();
                    slot = p.readByte(); // this is an int

                    var item = c.OnlinedCharacter.getInventory(InventoryType.ETC).getItem(slot);
                    if (item == null)
                    {
                        c.sendPacket(PacketCreator.enableActions());
                        return;
                    }

                    var itemId = item.getItemId();
                    if ((itemId != ItemId.INVITATION_CHAPEL && itemId != ItemId.INVITATION_CATHEDRAL) || !c.OnlinedCharacter.haveItem(itemId))
                    {
                        c.sendPacket(PacketCreator.enableActions());
                        return;
                    }

                    _weddingManager.TryInviteGuest(c.OnlinedCharacter, item, marriageId, name);
                    break;
                }

            case 6:
                {
                    // Open Wedding Invitation
                    slot = (byte)p.readInt();
                    int invitationItemId = p.readInt();
                    // 只有邀请函？如果收到多份邀请函如何区分是谁发送的？
                    var unknown1 = p.available();

                    if (invitationItemId == ItemId.RECEIVED_INVITATION_CHAPEL || invitationItemId == ItemId.RECEIVED_INVITATION_CATHEDRAL)
                    {
                        var item = c.OnlinedCharacter.getInventory(InventoryType.ETC).getItem(slot);
                        if (item == null || item.getItemId() != invitationItemId || int.TryParse(item.getGiftFrom(), out var weddingId))
                        {
                            c.sendPacket(PacketCreator.enableActions());
                            return;
                        }

                        _weddingManager.TryGetInvitationInfo(c.OnlinedCharacter, weddingId);
                    }

                    break;
                }

            case 9:
                try
                {
                    // By -- Dragoso (Drago)
                    // Groom and Bride's Wishlist

                    var player = c.OnlinedCharacter;

                    var eim = _weddingManager.GetMarriageInstance(player);
                    if (eim != null)
                    {
                        bool isMarrying = (player.getId() == eim.getIntProperty("groomId") || player.getId() == eim.getIntProperty("brideId"));

                        if (isMarrying)
                        {
                            int amount = p.readShort();
                            if (amount > 10)
                            {
                                amount = 10;
                            }

                            string wishlistItems = "";
                            for (int i = 0; i < amount; i++)
                            {
                                string s = p.readString();
                                wishlistItems += (s + "\r\n");
                            }

                            string wlKey;
                            if (player.getId() == eim.getIntProperty("groomId"))
                            {
                                wlKey = "groomWishlist";
                            }
                            else
                            {
                                wlKey = "brideWishlist";
                            }

                            if (eim.getProperty(wlKey) == "")
                            {
                                eim.setProperty(wlKey, wishlistItems);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }

                break;

            default:
                _logger.LogWarning("Unhandled RING_ACTION mode. Packet: {Packet}", p);
                break;
        }

        c.sendPacket(PacketCreator.enableActions());
    }
}
