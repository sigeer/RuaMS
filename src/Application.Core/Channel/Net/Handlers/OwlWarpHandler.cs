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
using Application.Resources.Messages;
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
        int mapObjectId = p.readInt();
        int mapid = p.readInt();

        // TODO: 如果一个玩家同时有雇佣商店+个人商店 应该找哪个？找找有没有其他参数
        var unknown = p.available();

        var map = c.CurrentServer.getMapFactory().getMap(mapid);
        var shop = map.getMapObject(mapObjectId) as IPlayerShop;
        if (shop == null)
        {
            c.sendPacket(PacketCreator.getOwlMessage(1));
            return;
        }

        if (c.OnlinedCharacter.Id == shop.OwnerId)
        {
            c.OnlinedCharacter.Popup(nameof(ClientMessage.PlayerShopt_OwlWarp_CannotVisitYourself));
            return;
        }

        c.OnlinedCharacter.changeMap(mapid);

        if (shop.Status.Is(PlayerShopStatus.Maintenance))
        {
            c.sendPacket(PacketCreator.getOwlMessage(18));
            return;
        }

        if (shop.BlackList.Contains(c.OnlinedCharacter.Name))
        {
            c.sendPacket(PacketCreator.getOwlMessage(17));
            return;
        }

        if (!shop.VisitShop(c.OnlinedCharacter))
        {
            c.sendPacket(PacketCreator.getOwlMessage(2));
            return;
        }
    }
}