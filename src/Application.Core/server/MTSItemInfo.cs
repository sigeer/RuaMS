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


using client.inventory;

namespace server;


/**
 * @author Traitor
 */
public class MTSItemInfo
{
    private int price;
    private Item item;
    private string seller;
    private int id;
    private int year, month, day = 1;

    public MTSItemInfo(Item item, int price, int id, int cid, string seller, string date)
    {
        var sellEnd = DateTimeOffset.Parse(date);

        this.item = item;
        this.price = price;
        this.seller = seller;
        this.id = id;
        this.year = sellEnd.Year;
        this.month = sellEnd.Month;
        this.day = sellEnd.Day;
    }

    public Item getItem()
    {
        return item;
    }

    public int getPrice()
    {
        return price;
    }

    public int getTaxes()
    {
        return 100 + price / 10;
    }

    public int getID()
    {
        return id;
    }

    public long getEndingDate()
    {
        return new DateTimeOffset(new DateTime(year, month, day)).ToUnixTimeMilliseconds();
    }

    public string getSeller()
    {
        return seller;
    }

    public static MTSItemInfo Map(MtsItem rs)
    {
        if (rs.Type != 1)
        {
            Item i = new Item(rs.Itemid, 0, (short)rs.Quantity);
            i.setOwner(rs.Owner);
            return (new MTSItemInfo(i, rs.Price, rs.Id, rs.Seller, rs.Sellername, rs.SellEnds));
        }
        else
        {
            Equip equip = new Equip(rs.Itemid, (byte)rs.Position, -1);
            equip.SetDataFromDB(rs);
            return (new MTSItemInfo(equip, rs.Price, rs.Id, rs.Seller, rs.Sellername, rs.SellEnds));
        }
    }
}
