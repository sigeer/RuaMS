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


using Application.Core.Channel.ServerData;
using tools;

namespace Application.Core.Channel.Net.Handlers;


/**
 * @author Ronan
 * <p>
 * Header layout thanks to Eric
 */
public class NewYearCardHandler : ChannelHandlerBase
{
    readonly NewYearCardService _manager;

    public NewYearCardHandler(NewYearCardService manager)
    {
        _manager = manager;
    }

    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        var player = c.OnlinedCharacter;
        byte reqMode = p.readByte();                 //[00] -> NewYearReq (0 = Send)

        if (reqMode == 0)
        {
            // card has been sent
            if (player.haveItem(ItemId.NEW_YEARS_CARD))
            {
                // new year's card
                short slot = p.readShort();                      //[00 2C] -> nPOS (Item Slot Pos)
                int itemid = p.readInt();                        //[00 20 F5 E5] -> nItemID (item id)

                int status = getValidNewYearCardStatus(itemid, player, slot);
                if (status == 0)
                {
                    // TODO: 数据传输可能存在延迟
                    if (player.canHold(ItemId.NEW_YEARS_CARD_SEND, 1))
                    {
                        string receiver = p.readString();  //[04 00 54 65 73 74] -> sReceiverName (person to send to)
                        string message = p.readString();

                        await _manager.SendNewYearCard(player, receiver, message);
                    }
                    else
                    {
                        player.sendPacket(PacketCreator.onNewYearCardRes(player, null, 5, 0x10));  // inventory full
                    }
                }
                else
                {
                    player.sendPacket(PacketCreator.onNewYearCardRes(player, null, 5, status));  // item and inventory errors
                }
            }
            else
            {
                player.sendPacket(PacketCreator.onNewYearCardRes(player, null, 5, 0x11));  // have no card to send
            }
        }
        else
        {
            //receiver accepted the card
            int cardid = p.readInt();

            if (player.canHold(ItemId.NEW_YEARS_CARD_RECEIVED, 1))
            {
                await _manager.AcceptNewYearCard(player, cardid);
            }
            else
            {
                player.sendPacket(PacketCreator.onNewYearCardRes(player, null, 5, 0x10));  // inventory full
            }
        }
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
