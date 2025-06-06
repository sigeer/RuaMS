/*
	This file is part of the OdinMS Maple Story Server
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
using Application.Core.Servers.Services;
using Application.Core.ServerTransports;

namespace server;

/**
 * @author Matze
 */
public class ShopFactory
{

    private Dictionary<int, Shop?> shops = new();
    private Dictionary<int, Shop?> npcShops = new();

    readonly ItemService _itemService;

    public ShopFactory(ItemService itemService)
    {
        _itemService = itemService;
    }

    private Shop? LoadShopFromRemote(int id, bool isShopId)
    {
        var ret = _itemService.GetShop(id, isShopId);
        if (ret != null)
        {
            shops.AddOrUpdate(ret.getId(), ret);
            npcShops.AddOrUpdate(ret.getNpcId(), ret);
        }
        else if (isShopId)
        {
            shops.AddOrUpdate(id, null);
        }
        else
        {
            npcShops.AddOrUpdate(id, null);
        }
        return ret;
    }

    public Shop? getShop(int shopId)
    {
        if (shops.TryGetValue(shopId, out var d))
            return d;

        return LoadShopFromRemote(shopId, true);
    }

    public Shop? getShopForNPC(int npcId)
    {
        if (npcShops.TryGetValue(npcId, out var d))
            return d;
        return LoadShopFromRemote(npcId, false);
    }

    public void reloadShops()
    {
        shops.Clear();
        npcShops.Clear();
    }
}
