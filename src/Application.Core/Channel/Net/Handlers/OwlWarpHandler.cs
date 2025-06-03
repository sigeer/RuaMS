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


using Application.Core.Game.Trades;
using constants.game;
using tools;

namespace Application.Core.Channel.Net.Handlers;

/*
 * @author Ronan
 */
public class OwlWarpHandler : ChannelHandlerBase
{

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        int ownerid = p.readInt();
        int mapid = p.readInt();

        if (ownerid == c.OnlinedCharacter.getId())
        {
            c.sendPacket(PacketCreator.serverNotice(1, "You cannot visit your own shop."));
            return;
        }

        var dto = c.CurrentServer.Transport.SendOwlWarp(mapid, ownerid, c.OnlinedCharacter.getOwlSearch());
        if (dto == null)
        {
            c.sendPacket(PacketCreator.getOwlMessage(1));
            return;
        }

        if (!dto.IsOpen)
        {
            c.sendPacket(PacketCreator.getOwlMessage(18));
            return;
        }


        if (GameConstants.isFreeMarketRoom(mapid))
        {
            if (dto.Channel == c.Channel)
            {
                c.OnlinedCharacter.changeMap(mapid);

                if (dto.IsOpen)
                {
                    IPlayerShop? ps = c.CurrentServer.PlayerShopManager.getPlayerShop(ownerid);
                    ps ??= c.CurrentServer.HiredMerchantManager.getHiredMerchant(ownerid);
                    if (ps == null)
                    {
                        c.sendPacket(PacketCreator.getOwlMessage(1));
                        return;
                    }

                    if (!ps.isOpen())
                    {
                        c.sendPacket(PacketCreator.getOwlMessage(18));
                        return;
                    }

                    if (ps is PlayerShop shop)
                    {
                        if (!shop.visitShop(c.OnlinedCharacter))
                        {
                            if (!shop.isBanned(c.OnlinedCharacter.getName()))
                            {
                                c.sendPacket(PacketCreator.getOwlMessage(2));
                            }
                            else
                            {
                                c.sendPacket(PacketCreator.getOwlMessage(17));
                            }
                        }
                    }
                    else if (ps is HiredMerchant merchant)
                    {
                        if (merchant.addVisitor(c.OnlinedCharacter))
                        {
                            c.sendPacket(PacketCreator.getHiredMerchant(c.OnlinedCharacter, merchant, false));
                            c.OnlinedCharacter.setHiredMerchant(merchant);
                        }
                        else
                        {
                            c.sendPacket(PacketCreator.getOwlMessage(2));
                        }
                    }
                }
                else
                {
                    //c.sendPacket(PacketCreator.serverNotice(1, "That merchant has either been closed or is under maintenance."));
                    c.sendPacket(PacketCreator.getOwlMessage(18));
                }
            }
            else
            {
                c.sendPacket(PacketCreator.serverNotice(1, $"That {dto.TypeName} is currently located in another channel. Current location: Channel {dto.Channel}, '{dto.MapName}'."));
            }
        }
        else
        {
            c.sendPacket(PacketCreator.serverNotice(1, $"That {dto.TypeName} is currently located outside of the FM area. Current location: Channel {dto.Channel}, '{dto.MapName}'."));
        }
    }
}