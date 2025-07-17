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


using Application.Core.Channel.Services;
using Application.Core.Game.Trades;
using constants.game;
using tools;

namespace Application.Core.Channel.Net.Handlers;

/*
 * @author Ronan
 */
public class OwlWarpHandler : ChannelHandlerBase
{
    readonly PlayerShopService _service;

    public OwlWarpHandler(PlayerShopService service)
    {
        _service = service;
    }

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        int ownerid = p.readInt();
        int mapid = p.readInt();

        if (ownerid == c.OnlinedCharacter.getId())
        {
            c.sendPacket(PacketCreator.serverNotice(1, "You cannot visit your own shop."));
            return;
        }

        // TODO: 如果一个玩家同时有雇佣商店+个人商店 应该找哪个？找找有没有其他参数
        var unknown = p.available();

        var dto = c.CurrentServerContainer.Transport.SendOwlWarp(mapid, ownerid, c.OnlinedCharacter.getOwlSearch());
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
                    IPlayerShop? ps = c.CurrentServer.PlayerShopManager.GetPlayerShop((PlayerShopType)dto.Type, ownerid);
                    if (ps == null)
                    {
                        c.sendPacket(PacketCreator.getOwlMessage(1));
                        return;
                    }

                    if (!ps.Status.Is(PlayerShopStatus.Opening))
                    {
                        c.sendPacket(PacketCreator.getOwlMessage(18));
                        return;
                    }

                    if (ps is PlayerShop shop)
                    {
                        if (!shop.VisitShop(c.OnlinedCharacter))
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
                        if (!merchant.VisitShop(c.OnlinedCharacter))
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
                c.sendPacket(PacketCreator.serverNotice(1, $"That {(PlayerShopType)dto.Type} is currently located in another channel. Current location: Channel {dto.Channel}, '{dto.MapName}'."));
            }
        }
        else
        {
            c.sendPacket(PacketCreator.serverNotice(1, $"That {(PlayerShopType)dto.Type} is currently located outside of the FM area. Current location: Channel {dto.Channel}, '{dto.MapName}'."));
        }
    }
}