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


using Application.Core.Managers;
using client.inventory;
using client.inventory.manipulator;
using client.processor.npc;
using constants.id;
using net.packet;
using service;
using tools;
using tools.packets;

namespace net.server.channel.handlers;


/**
 * @author Jvlaple
 * @author Ronan - major overhaul on Ring handling mechanics
 * @author Drago (Dragohe4rt) - on Wishlist
 */
public class RingActionHandler : AbstractPacketHandler
{

    private NoteService noteService;

    public RingActionHandler(NoteService noteService)
    {
        this.noteService = noteService;
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

    public static void sendEngageProposal(IClient c, string name, int itemid)
    {
        int newBoxId = getEngagementBoxId(itemid);
        var target = c.getChannelServer().getPlayerStorage().getCharacterByName(name);
        var source = c.OnlinedCharacter;

        // TODO: get the correct packet bytes for these popups
        if (source.isMarried())
        {
            source.dropMessage(1, "You're already married!");
            source.sendPacket(WeddingPackets.OnMarriageResult(0));
            return;
        }
        else if (source.getPartnerId() > 0)
        {
            source.dropMessage(1, "You're already engaged!");
            source.sendPacket(WeddingPackets.OnMarriageResult(0));
            return;
        }
        else if (source.getMarriageItemId() > 0)
        {
            source.dropMessage(1, "You're already engaging someone!");
            source.sendPacket(WeddingPackets.OnMarriageResult(0));
            return;
        }
        else if (target == null)
        {
            source.dropMessage(1, "Unable to find " + name + " on this channel.");
            source.sendPacket(WeddingPackets.OnMarriageResult(0));
            return;
        }
        else if (target == source)
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
        else if (source.getLevel() < 50)
        {
            source.dropMessage(1, "You can only propose being level 50 or higher.");
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
        else if (target.isMarried())
        {
            source.dropMessage(1, "The player is already married!");
            source.sendPacket(WeddingPackets.OnMarriageResult(0));
            return;
        }
        else if (target.getPartnerId() > 0 || target.getMarriageItemId() > 0)
        {
            source.dropMessage(1, "The player is already engaged!");
            source.sendPacket(WeddingPackets.OnMarriageResult(0));
            return;
        }
        else if (target.haveWeddingRing())
        {
            source.dropMessage(1, "The player already holds a marriage ring...");
            source.sendPacket(WeddingPackets.OnMarriageResult(0));
            return;
        }
        else if (source.haveWeddingRing())
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
        else if (!InventoryManipulator.checkSpace(target.getClient(), newBoxId + 1, 1, ""))
        {
            source.dropMessage(5, "The girl you proposed doesn't have a ETC slot available right now.");
            source.sendPacket(WeddingPackets.OnMarriageResult(0));
            return;
        }

        source.setMarriageItemId(itemid);
        target.sendPacket(WeddingPackets.onMarriageRequest(source.getName(), source.getId()));
    }







    public override void HandlePacket(InPacket p, IClient c)
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
                if (c.OnlinedCharacter.getMarriageItemId() / 1000000 != 4)
                {
                    c.OnlinedCharacter.setMarriageItemId(-1);
                }
                break;

            case 2: // Accept/Deny Proposal
                bool accepted = p.readByte() > 0;
                name = p.readString();
                int id = p.readInt();

                var source = c.getWorldServer().getPlayerStorage().getCharacterByName(name);
                var target = c.OnlinedCharacter;

                if (source == null || !source.IsOnlined)
                {
                    target.sendPacket(PacketCreator.enableActions());
                    return;
                }

                int itemid = source.getMarriageItemId();
                if (target.getPartnerId() > 0 || source.getId() != id || itemid <= 0 || !source.haveItem(itemid) || source.getPartnerId() > 0 || !source.isAlive() || !target.isAlive())
                {
                    target.sendPacket(PacketCreator.enableActions());
                    return;
                }

                if (accepted)
                {
                    int newItemId = getEngagementBoxId(itemid);
                    if (!InventoryManipulator.checkSpace(c, newItemId, 1, "") || !InventoryManipulator.checkSpace(source.getClient(), newItemId, 1, ""))
                    {
                        target.sendPacket(PacketCreator.enableActions());
                        return;
                    }

                    try
                    {
                        InventoryManipulator.removeById(source.getClient(), InventoryType.USE, itemid, 1, false, false);

                        int tempMarriageId = c.getWorldServer().createRelationship(source.getId(), target.getId());
                        source.setPartnerId(target.getId()); // engage them (new marriageitemid, partnerid for both)
                        target.setPartnerId(source.getId());

                        source.setMarriageItemId(newItemId);
                        target.setMarriageItemId(newItemId + 1);

                        InventoryManipulator.addById(source.getClient(), newItemId, 1);
                        InventoryManipulator.addById(c, (newItemId + 1), 1);

                        source.sendPacket(WeddingPackets.OnMarriageResult(tempMarriageId, source, false));
                        target.sendPacket(WeddingPackets.OnMarriageResult(tempMarriageId, source, false));

                        source.sendPacket(WeddingPackets.OnNotifyWeddingPartnerTransfer(target.getId(), target.getMapId()));
                        target.sendPacket(WeddingPackets.OnNotifyWeddingPartnerTransfer(source.getId(), source.getMapId()));
                    }
                    catch (Exception e)
                    {
                        log.Error(e, "Error with engagement");
                    }
                }
                else
                {
                    source.dropMessage(1, "She has politely declined your engagement request.");
                    source.sendPacket(WeddingPackets.OnMarriageResult(0));

                    source.setMarriageItemId(-1);
                }
                break;

            case 3: // Break Engagement
                RingManager.BreakMarriageRing(c.OnlinedCharacter, p.readInt());
                break;

            case 5: // Invite %s to Wedding
                name = p.readString();
                int marriageId = p.readInt();
                slot = p.readByte(); // this is an int

                int itemId;
                try
                {
                    itemId = c.OnlinedCharacter.getInventory(InventoryType.ETC).getItem(slot)?.getItemId() ?? 0;
                }
                catch (NullReferenceException e)
                {
                    log.Error(e.ToString());
                    c.sendPacket(PacketCreator.enableActions());
                    return;
                }

                if ((itemId != ItemId.INVITATION_CHAPEL && itemId != ItemId.INVITATION_CATHEDRAL) || !c.OnlinedCharacter.haveItem(itemId))
                {
                    c.sendPacket(PacketCreator.enableActions());
                    return;
                }

                string groom = c.OnlinedCharacter.getName();
                string bride = CharacterManager.getNameById(c.OnlinedCharacter.getPartnerId());
                int guest = CharacterManager.getIdByName(name);
                if (string.IsNullOrEmpty(groom) || string.IsNullOrEmpty(bride) || guest <= 0)
                {
                    c.OnlinedCharacter.dropMessage(5, "Unable to find " + name + "!");
                    return;
                }

                try
                {
                    var wserv = c.getWorldServer();
                    var registration = wserv.getMarriageQueuedLocation(marriageId);

                    if (registration != null)
                    {
                        if (wserv.addMarriageGuest(marriageId, guest))
                        {
                            bool cathedral = registration.Value.Key;
                            int newItemId = cathedral ? ItemId.RECEIVED_INVITATION_CATHEDRAL : ItemId.RECEIVED_INVITATION_CHAPEL;

                            var cserv = c.getChannelServer();
                            int resStatus = cserv.getWeddingReservationStatus(marriageId, cathedral);
                            if (resStatus > 0)
                            {
                                long expiration = cserv.getWeddingTicketExpireTime(resStatus + 1);

                                string baseMessage = $"You've been invited to {groom} and {bride}'s Wedding!";
                                var guestChr = c.getWorldServer().getPlayerStorage().getCharacterById(guest);
                                if (guestChr != null && guestChr.isLoggedinWorld()
                                    && InventoryManipulator.checkSpace(guestChr.getClient(), newItemId, 1, "")
                                    && InventoryManipulator.addById(guestChr.getClient(), newItemId, 1, expiration: expiration))
                                {
                                    guestChr.dropMessage(6, $"[Wedding] {baseMessage}");
                                }
                                else
                                {
                                    string dueyMessage = baseMessage + " Receive your invitation from Duey!";
                                    if (guestChr != null && guestChr.isLoggedinWorld())
                                    {
                                        guestChr.dropMessage(6, $"[Wedding] {dueyMessage}");
                                    }
                                    else
                                    {
                                        noteService.sendNormal(dueyMessage, groom, name);
                                    }

                                    Item weddingTicket = new Item(newItemId, 0, 1);
                                    weddingTicket.setExpiration(expiration);

                                    DueyProcessor.dueyCreatePackage(weddingTicket, 0, groom, guest);
                                }
                            }
                            else
                            {
                                c.OnlinedCharacter.dropMessage(5, "Wedding is already under way. You cannot invite any more guests for the event.");
                            }
                        }
                        else
                        {
                            c.OnlinedCharacter.dropMessage(5, "'" + name + "' is already invited for your marriage.");
                        }
                    }
                    else
                    {
                        c.OnlinedCharacter.dropMessage(5, "Invitation was not sent to '" + name + "'. Either the time for your marriage reservation already came or it was not found.");
                    }

                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex.ToString());
                    return;
                }

                c.getAbstractPlayerInteraction().gainItem(itemId, -1);
                break;

            case 6: // Open Wedding Invitation
                slot = (byte)p.readInt();
                int invitationid = p.readInt();

                if (invitationid == ItemId.RECEIVED_INVITATION_CHAPEL || invitationid == ItemId.RECEIVED_INVITATION_CATHEDRAL)
                {
                    var item = c.OnlinedCharacter.getInventory(InventoryType.ETC).getItem(slot);
                    if (item == null || item.getItemId() != invitationid)
                    {
                        c.sendPacket(PacketCreator.enableActions());
                        return;
                    }

                    // collision case: most soon-to-come wedding will show up
                    var coupleId = c.getWorldServer().getWeddingCoupleForGuest(c.OnlinedCharacter.getId(), invitationid == ItemId.RECEIVED_INVITATION_CATHEDRAL);
                    if (coupleId != null)
                    {
                        int groomId = coupleId.HusbandId, brideId = coupleId.WifeId;
                        c.sendPacket(WeddingPackets.sendWeddingInvitation(CharacterManager.getNameById(groomId), CharacterManager.getNameById(brideId)));
                    }
                }

                break;

            case 9:
                try
                {
                    // By -- Dragoso (Drago)
                    // Groom and Bride's Wishlist

                    var player = c.OnlinedCharacter;

                    var eim = player.getEventInstance();
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
                    log.Error(ex.ToString());
                }

                break;

            default:
                log.Warning("Unhandled RING_ACTION mode. Packet: {Packet}", p);
                break;
        }

        c.sendPacket(PacketCreator.enableActions());
    }
}
