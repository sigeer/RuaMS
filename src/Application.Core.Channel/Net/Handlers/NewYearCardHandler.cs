/*
    This file is part of the HeavenMS MapleStory NewServer
    Copyleft (L) 2016 - 2019 RonanLana

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


using Application.Core.Game.Players;
using Application.EF;
using client.newyear;
using constants.id;
using constants.inventory;
using net.packet;
using net.server;
using tools;

namespace Application.Core.Channel.Net.Handlers;


/**
 * @author Ronan
 * <p>
 * Header layout thanks to Eric
 */
public class NewYearCardHandler : ChannelHandlerBase
{

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        var player = c.OnlinedCharacter;
        byte reqMode = p.readByte();                 //[00] -> NewYearReq (0 = Send)

        if (reqMode == 0)
        {  // card has been sent
            if (player.haveItem(ItemId.NEW_YEARS_CARD))
            {  // new year's card
                short slot = p.readShort();                      //[00 2C] -> nPOS (Item Slot Pos)
                int itemid = p.readInt();                        //[00 20 F5 E5] -> nItemID (item id)

                int status = getValidNewYearCardStatus(itemid, player, slot);
                if (status == 0)
                {
                    if (player.canHold(ItemId.NEW_YEARS_CARD_SEND, 1))
                    {
                        string receiver = p.readString();  //[04 00 54 65 73 74] -> sReceiverName (person to send to)

                        int receiverid = getReceiverId(receiver);
                        if (receiverid != -1)
                        {
                            if (receiverid != c.OnlinedCharacter.getId())
                            {
                                string message = p.readString();   //[06 00 4C 65 74 74 65 72] -> sContent (message)

                                NewYearCardRecord newyear = new NewYearCardRecord(player.getId(), player.getName(), receiverid, receiver, message);
                                NewYearCardRecord.saveNewYearCard(newyear);
                                player.addNewYearRecord(newyear);

                                player.getAbstractPlayerInteraction().gainItem(ItemId.NEW_YEARS_CARD, -1);
                                player.getAbstractPlayerInteraction().gainItem(ItemId.NEW_YEARS_CARD_SEND, 1);

                                Server.getInstance().setNewYearCard(newyear);
                                newyear.startNewYearCardTask();
                                player.sendPacket(PacketCreator.onNewYearCardRes(player, newyear, 4, 0));    // successfully sent
                            }
                            else
                            {
                                player.sendPacket(PacketCreator.onNewYearCardRes(player, -1, 5, 0xF));   // cannot send to yourself
                            }
                        }
                        else
                        {
                            player.sendPacket(PacketCreator.onNewYearCardRes(player, -1, 5, 0x13));  // cannot find such character
                        }
                    }
                    else
                    {
                        player.sendPacket(PacketCreator.onNewYearCardRes(player, -1, 5, 0x10));  // inventory full
                    }
                }
                else
                {
                    player.sendPacket(PacketCreator.onNewYearCardRes(player, -1, 5, status));  // item and inventory errors
                }
            }
            else
            {
                player.sendPacket(PacketCreator.onNewYearCardRes(player, -1, 5, 0x11));  // have no card to send
            }
        }
        else
        {    //receiver accepted the card
            int cardid = p.readInt();

            var newyear = NewYearCardRecord.loadNewYearCard(cardid);

            if (newyear != null && newyear.getReceiverId() == player.getId() && !newyear.isReceiverCardReceived())
            {
                if (!newyear.isSenderCardDiscarded())
                {
                    if (player.canHold(ItemId.NEW_YEARS_CARD_RECEIVED, 1))
                    {
                        newyear.stopNewYearCardTask();
                        NewYearCardRecord.updateNewYearCard(newyear);

                        player.getAbstractPlayerInteraction().gainItem(ItemId.NEW_YEARS_CARD_RECEIVED, 1);
                        if (newyear.getMessage().Count() > 0)
                        {
                            player.dropMessage(6, "[New Year] " + newyear.getSenderName() + ": " + newyear.getMessage());
                        }

                        player.addNewYearRecord(newyear);
                        player.sendPacket(PacketCreator.onNewYearCardRes(player, newyear, 6, 0));    // successfully rcvd

                        player.getMap().broadcastMessage(PacketCreator.onNewYearCardRes(player, newyear, 0xD, 0));

                        var sender = c.getWorldServer().getPlayerStorage().getCharacterById(newyear.getSenderId());
                        if (sender != null && sender.isLoggedinWorld())
                        {
                            sender.getMap().broadcastMessage(PacketCreator.onNewYearCardRes(sender, newyear, 0xD, 0));
                            sender.dropMessage(6, "[New Year] Your addressee successfully received the New Year card.");
                        }
                    }
                    else
                    {
                        player.sendPacket(PacketCreator.onNewYearCardRes(player, -1, 5, 0x10));  // inventory full
                    }
                }
                else
                {
                    player.dropMessage(6, "[New Year] The sender of the New Year card already dropped it. Nothing to receive.");
                }
            }
            else
            {
                if (newyear == null)
                {
                    player.dropMessage(6, "[New Year] The sender of the New Year card already dropped it. Nothing to receive.");
                }
            }
        }
    }

    private static int getReceiverId(string receiver)
    {
        using var dbContext = new DBContext();
        return dbContext.Characters.Where(x => x.Name == receiver && x.World == 0).Select(x => new { x.Id })?.FirstOrDefault()?.Id ?? -1;
    }


    private static int getValidNewYearCardStatus(int itemid, IPlayer player, short slot)
    {
        if (!ItemConstants.isNewYearCardUse(itemid))
        {
            return 0x14;
        }

        var it = player.getInventory(ItemConstants.getInventoryType(itemid)).getItem(slot);
        return (it != null && it.getItemId() == itemid) ? 0 : 0x12;
    }
}
